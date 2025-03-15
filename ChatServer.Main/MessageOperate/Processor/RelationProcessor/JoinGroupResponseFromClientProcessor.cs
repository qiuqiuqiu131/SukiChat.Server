using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services;
using ChatServer.Main.Services.Helper;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace ChatServer.Main.MessageOperate.Processor.RelationProcessor;

/// <summary>
/// 处理目标：
/// JoinGroupResponseFromClient 入群请求回复
/// 
/// 数据库操作：
/// 1、GroupRequest 更新请求结果
/// 2、GroupRelation 获取群组的管理员们
/// 
/// 需要发送的消息：
/// 1、JoinGroupResponseFromServer (To:请求者) 通知请求者是否成功加入群聊
/// 2、JoinGroupResponseResponseFromServer (To:管理员) 通知管理员此请求已经被UserId处理
/// </summary>
public class JoinGroupResponseFromClientProcessor : IProcessor<JoinGroupResponseFromClient>
{
    private readonly IClientChannelManager clientChannelManager;
    private readonly IServiceProvider serviceProvider;
    private readonly IUnitOfWork unitOfWork;

    public JoinGroupResponseFromClientProcessor(IClientChannelManager clientChannelManager,
        IServiceProvider serviceProvider,
        IUnitOfWork unitOfWork)
    {
        this.clientChannelManager = clientChannelManager;
        this.serviceProvider = serviceProvider;
        this.unitOfWork = unitOfWork;
    }

    public async Task Process(MessageUnit<JoinGroupResponseFromClient> unit)
    {
        unit.Channel.TryGetTarget(out IChannel? channel);

        var message = unit.Message;

        // 检查groupRequest是否未被处理
        var groupRequestRepository = unitOfWork.GetRepository<GroupRequest>();
        var groupRequest = await groupRequestRepository.GetFirstOrDefaultAsync(predicate: d => d.Id.Equals(message.RequestId), disableTracking: false);
        if (groupRequest == null)
        {
            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new JoinGroupResponseResponseFromServer
                {
                    Response = new CommonResponse { State = false, Message = "不存在此请求" }
                });
            }
            return;
        }
        else if (groupRequest.IsSolved)
        {
            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new JoinGroupResponseResponseFromServer
                {
                    Response = new CommonResponse { State = false, Message = "此请求已被处理" }
                });
            }
            return;
        }

        // 验证用户是否为此群组的Manager
        var groupService1 = serviceProvider.GetRequiredService<IGroupService>();
        var isManager = await groupService1.IsGroupManager(message.UserId, groupRequest.GroupId);
        if (!isManager)
        {
            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new JoinGroupResponseResponseFromServer
                {
                    Response = new CommonResponse { State = false, Message = "非群组管理员" }
                });
            }
            return;
        }

        bool isMember = false;

        // 更改数据库，保存记录
        try
        {
            var now = DateTime.Now;

            groupRequest.IsSolved = true;
            groupRequest.IsAccept = message.Accept;
            groupRequest.AcceptByUserId = message.UserId;
            groupRequest.SolveTime = now;
            await unitOfWork.SaveChangesAsync();
            groupRequestRepository.ChangeEntityState(groupRequest, Microsoft.EntityFrameworkCore.EntityState.Detached);

            if(message.Accept)
            {
                // 添加群成员,如果已经添加了，那么跳过
                var groupRelationRepository = unitOfWork.GetRepository<GroupRelation>();
                var entity = await groupRelationRepository.GetFirstOrDefaultAsync(predicate: d => d.GroupId.Equals(groupRequest.GroupId) && d.UserId.Equals(groupRequest.UserFromId));
                if (entity == null)
                {
                    await groupRelationRepository.InsertAsync(new GroupRelation
                    {
                        GroupId = groupRequest.GroupId,
                        UserId = groupRequest.UserFromId,
                        Grouping = "默认分组",
                        Status = 2,
                        JoinTime = now
                    });
                }
                else
                    isMember = true;
                await unitOfWork.SaveChangesAsync();
            }
        }
        catch
        {
            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new JoinGroupResponseResponseFromServer
                {
                    Response = new CommonResponse { State = false, Message = "服务器出错" }
                });
            }
            return;
        }



        // 成功保存记录
        // 发送给请求者，发送请求回应
        var sender = clientChannelManager.GetClient(groupRequest.UserFromId);
        if(sender != null)
        {
            await sender.WriteAndFlushProtobufAsync(new JoinGroupResponseFromServer
            {
                Accept = groupRequest.IsAccept,
                Time = groupRequest.SolveTime.ToString(),
                UserIdFrom = groupRequest.AcceptByUserId,
                UserIdTarget = groupRequest.UserFromId,
                RequestId = groupRequest.Id
            });
        }



        // 发送给管理员，此请求已经被处理
        var response = new JoinGroupResponseResponseFromServer
        {
            Response = new CommonResponse { State = true },
            RequestId = groupRequest.Id,
            UserId = groupRequest.AcceptByUserId,
            Time = groupRequest.SolveTime.ToString(),
            Accept = groupRequest.IsAccept
        };


        var groupService = serviceProvider.GetRequiredService<IGroupService>();

        List<string> managerIds = await groupService.GetGroupManagers(groupRequest.GroupId);
        var managerChannels = managerIds.AsParallel().Select(id =>
        {
            var managerChannel = clientChannelManager.GetClient(id);
            return new { Id = id, Channel = managerChannel };
        }).ToList();

        foreach (var managerChannel in managerChannels)
        {
            if (managerChannel.Channel != null)
                await managerChannel.Channel.WriteAndFlushProtobufAsync(response);
        }
        managerChannels.Clear();

        if (isMember || !message.Accept)
            return;

        // 通知群成员，有新成员加入
        var newMember = new NewMemberJoinMessage
        {
            UserId = groupRequest.UserFromId,
            GroupId = groupRequest.GroupId,
            Time = groupRequest.SolveTime.ToString()
        };

        List<string> memberIds = await groupService.GetGroupMembers(groupRequest.GroupId);
        var memberChannels = new List<IChannel>();
        foreach(var memberId in memberIds)
        {
            var client = clientChannelManager.GetClient(memberId);
            if(client != null)
                memberChannels.Add(client);
        }

        foreach (var memberChannel in memberChannels)
            await memberChannel.WriteAndFlushProtobufAsync(newMember);
       


        // 发送群通知消息
        var userService = serviceProvider.GetRequiredService<IUserService>();
        var user = await userService.GetUser(groupRequest.UserFromId);
        var chatMessage = new GroupChatMessage
        {
            GroupId = groupRequest.GroupId,
            UserFromId = "System",
            Time = DateTime.Now.ToString(),
        };
        chatMessage.Messages.Add(new ChatMessage
        {
            SystemMessage = new SystemMessage
            {
                Blocks =
                {
                    new SystemMessageBlock{Text = user.Name,Bold = true},
                    new SystemMessageBlock{Text = "加入了群聊"}
                }
            }
        });

        // 保存到数据库
        ChatGroup chatGroup = new ChatGroup
        {
            UserFromId = "System",
            GroupId = groupRequest.GroupId,
            Message = ChatMessageHelper.EncruptChatMessage(chatMessage.Messages),
            Time = DateTime.Now,
        };
        var respository = unitOfWork.GetRepository<ChatGroup>();
        await respository.InsertAsync(chatGroup);
        await unitOfWork.SaveChangesAsync();

        chatMessage.Id = chatGroup.Id;

        foreach (var memberChannel in memberChannels)
        {
            await memberChannel.WriteAndFlushProtobufAsync(chatMessage);
        }
    }
}
