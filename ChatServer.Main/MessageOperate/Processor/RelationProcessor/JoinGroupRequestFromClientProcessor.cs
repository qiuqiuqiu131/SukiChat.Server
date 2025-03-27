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
/// JoinGroupRequestFromClient 申请加入群聊请求
/// 
/// 数据库操作：
/// 1、GroupRelation 群组关系，用于获取管理员和群主Id
/// 2、GroupRequest 保存请求
/// 
/// 需要发送的消息：
/// 1、JoinGroupRequestResponseFromServer (To:请求者) 通知请求者是否请求成功
/// 2、JoinGroupRequestFromServer (To:管理员和群主）通知有新的入群请求
/// </summary>
public class JoinGroupRequestFromClientProcessor : IProcessor<JoinGroupRequestFromClient>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IUserService userService;
    private readonly IGroupService groupService;
    private readonly IClientChannelManager clientChannelManager;

    public JoinGroupRequestFromClientProcessor(IUnitOfWork unitOfWork,
        IUserService userService,
        IGroupService groupService,
        IClientChannelManager clientChannelManager)
    {
        this.unitOfWork = unitOfWork;
        this.userService = userService;
        this.groupService = groupService;
        this.clientChannelManager = clientChannelManager;
    }

    public async Task Process(MessageUnit<JoinGroupRequestFromClient> unit)
    {
        unit.Channel.TryGetTarget(out IChannel? channel);
        var message = unit.Message;

        // 检查userId是否存在
        bool userExist = await userService.IsUserExist(message.UserId);
        if (!userExist)
        {
            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new JoinGroupRequestResponseFromServer
                {
                    Response = new CommonResponse { State = false, Message = "用户不存在" }
                });
            }
            return;
        }

        // 检查group是否存在
        bool groupExist = await groupService.IsGroupExist(message.GroupId);
        if (!groupExist)
        {
            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new JoinGroupRequestResponseFromServer
                {
                    Response = new CommonResponse { State = false, Message = "群聊不存在" }
                });
            }
            return;
        }

        bool isMember = await groupService.IsGroupMember(message.UserId, message.GroupId);
        if (isMember)
        {
            if(channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new JoinGroupRequestResponseFromServer
                {
                    Response = new CommonResponse { State = false, Message = "您已经是此群成员了" }
                });
            }
            return;
        }

        var groupRequest = new GroupRequest
        {
            GroupId = message.GroupId,
            UserFromId = message.UserId,
            RequestTime = DateTime.Now,
            Message = message.Message,
            Grouping = message.Grouping,
            NickName = message.NickName,
            Remark = message.Remark
        };

        try
        {
            var groupReuqestRepository = unitOfWork.GetRepository<GroupRequest>();
            groupReuqestRepository.Update(groupRequest);
            await unitOfWork.SaveChangesAsync();
        }
        catch
        {
            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new JoinGroupRequestResponseFromServer
                {
                    Response = new CommonResponse { State = false, Message = "服务器出错" }
                });
            }
            return;
        }

        // 数据库处理成功，返回请求成功消息
        if (channel != null)
        {
            await channel.WriteAndFlushProtobufAsync(new JoinGroupRequestResponseFromServer
            {
                Response = new CommonResponse { State = true, Message = "请求成功" },
                GroupId = message.GroupId,
                RequestId = groupRequest.Id,
                Time = groupRequest.RequestTime.ToString()
            });
        }

        var response = new JoinGroupRequestFromServer
        {
            RequestId = groupRequest.Id,
            UserId = message.UserId,
            GroupId = message.GroupId,
            Time = groupRequest.RequestTime.ToString(),
            Message = message.Message,
        };

        // 寻找此群的管理员和群主
        List<string> managerIds = await groupService.GetGroupManagers(message.GroupId);
        var managerChannels = managerIds.AsParallel().Select(id =>
        {
            var managerChannel = clientChannelManager.GetClient(id);
            return new { Id = id, Channel = managerChannel };
        }).ToList();

        foreach (var managerChannel in managerChannels)
        {
            if(managerChannel.Channel != null)
                _ = managerChannel.Channel.WriteAndFlushProtobufAsync(response);
        }
    }
}
