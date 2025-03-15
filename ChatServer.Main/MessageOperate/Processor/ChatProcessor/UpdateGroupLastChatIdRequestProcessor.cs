using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.ChatProcessor
{
    class UpdateGroupLastChatIdRequestProcessor : IProcessor<UpdateGroupLastChatIdRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IGroupService groupService;

        public UpdateGroupLastChatIdRequestProcessor(IUnitOfWork unitOfWork,
            IUserService userService,
            IGroupService groupService)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.groupService = groupService;
        }

        public async Task Process(MessageUnit<UpdateGroupLastChatIdRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            // 验证是否为用户
            if(!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new UpdateGroupLastChatIdResponse 
                            { 
                                Response = new CommonResponse { State = false, Message = "非法账号" } 
                            });
                return;
            }

            // 验证群组是否存在
            if (!await groupService.IsGroupExist(message.GroupId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new UpdateGroupLastChatIdResponse
                    {
                        Response = new CommonResponse { State = false, Message = "群聊不存在" }
                    });
                return;
            }

            // 验证是否为组员关系
            if(! await groupService.IsGroupMember(message.UserId,message.GroupId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new UpdateGroupLastChatIdResponse
                    {
                        Response = new CommonResponse { State = false, Message = "您非本群成员" }
                    });
                return;
            }

            try
            {
                var groupRelationRepository = unitOfWork.GetRepository<GroupRelation>();
                var relation = await groupRelationRepository.GetFirstOrDefaultAsync(predicate: d => d.GroupId.Equals(message.GroupId) && d.UserId.Equals(message.UserId), disableTracking: false);
                if (relation != null)
                {
                    if(relation.LastChatId < message.LastChatId)
                        relation.LastChatId = message.LastChatId;
                    await unitOfWork.SaveChangesAsync();
                    groupRelationRepository.ChangeEntityState(relation,EntityState.Detached);
                }
            }
            catch
            {
                // doNothing
            }

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new UpdateGroupLastChatIdResponse
                {
                    Response = new CommonResponse
                    {
                        State = true
                    }
                });
        }
    }
}
