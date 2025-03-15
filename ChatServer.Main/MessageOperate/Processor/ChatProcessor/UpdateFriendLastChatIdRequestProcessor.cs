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
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.ChatProcessor
{
    class UpdateFriendLastChatIdRequestProcessor : IProcessor<UpdateFriendLastChatIdRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IFriendService friendService;

        public UpdateFriendLastChatIdRequestProcessor(IUnitOfWork unitOfWork,
            IUserService userService,
            IFriendService friendService)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.friendService = friendService;
        }

        public async Task Process(MessageUnit<UpdateFriendLastChatIdRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            // 验证是否为用户
            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new UpdateFriendLastChatIdResponse
                    {
                        Response = new CommonResponse { State = false, Message = "非法账号" }
                    });
                return;
            }

            // 验证群组是否存在
            if (!await userService.IsUserExist(message.FriendId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new UpdateFriendLastChatIdResponse
                    {
                        Response = new CommonResponse { State = false, Message = "好友不存在" }
                    });
                return;
            }

            // 验证是否为组员关系
            if (!await friendService.IsFriend(message.UserId, message.FriendId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new UpdateFriendLastChatIdResponse
                    {
                        Response = new CommonResponse { State = false, Message = "你们不是好友" }
                    });
                return;
            }

            try
            {
                var friendRelationRepository = unitOfWork.GetRepository<FriendRelation>();
                var friendRelation = await friendRelationRepository.GetFirstOrDefaultAsync(predicate:d => d.User1Id.Equals(message.UserId) && d.User2Id.Equals(message.FriendId),disableTracking:false);
                if(friendRelation != null)
                {
                    if(friendRelation.LastChatId < message.LastChatId)
                        friendRelation.LastChatId = message.LastChatId;
                    await unitOfWork.SaveChangesAsync();
                    friendRelationRepository.ChangeEntityState(friendRelation, EntityState.Detached);
                }
            }
            catch
            {
                // doNothing
            }

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new UpdateFriendLastChatIdResponse
                {
                    Response = new CommonResponse { State = true }
                });
        }
    }
}
