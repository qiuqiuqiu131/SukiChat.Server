using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    private readonly IGroupService groupService;
    private readonly IUnitOfWork unitOfWork;

    public JoinGroupResponseFromClientProcessor(IClientChannelManager clientChannelManager,
        IGroupService groupService,
        IUnitOfWork unitOfWork)
    {
        this.clientChannelManager = clientChannelManager;
        this.groupService = groupService;
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
        var isManager = await groupService.IsGroupManager(message.UserId, groupRequest.GroupId);
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

        // 更改数据库，保存记录
        try
        {
            groupRequest.IsSolved = true;
            groupRequest.IsAccept = message.Accept;
            groupRequest.AcceptByUserId = message.UserId;
            groupRequest.SolveTime = DateTime.Now;
            await unitOfWork.SaveChangesAsync();
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
            Time = groupRequest.SolveTime.ToString()
        };

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

        // 通知群成员，有新成员加入
        var newMember = new NewMemberJoinMessage
        {
            UserId = groupRequest.UserFromId,
            Time = groupRequest.SolveTime.ToString()
        };

        List<string> memberIds = await groupService.GetGroupMembers(groupRequest.GroupId);
        var memberChannels = managerIds.AsParallel().Select(id =>
        {
            var managerChannel = clientChannelManager.GetClient(id);
            return new { Id = id, Channel = managerChannel };
        }).ToList();

        foreach (var memberChannel in memberChannels)
        {
            if (memberChannel.Channel != null)
                await memberChannel.Channel.WriteAndFlushProtobufAsync(newMember);
        }
    }
}
