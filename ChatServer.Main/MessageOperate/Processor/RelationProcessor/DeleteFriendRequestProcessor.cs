using ChatServer.Common;
using ChatServer.Common.Helper;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.RelationProcessor
{
    class DeleteFriendRequestProcessor : IProcessor<DeleteFriendRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IFriendService friendService;
        private readonly IClientChannelManager clientChannelManager;

        public DeleteFriendRequestProcessor(IUnitOfWork unitOfWork,
            IUserService userService,
            IClientChannelManager clientChannelManager,
            IFriendService friendService)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.friendService = friendService;
            this.clientChannelManager = clientChannelManager;
        }

        public async Task Process(MessageUnit<DeleteFriendRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            // 验证是否为注册用户
            if (!await userService.IsUserExist(message.UserId) ||
                !await userService.IsUserExist(message.FriendId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new DeleteFriendMessage
                    {
                        Response = new CommonResponse { State = false, Message = "非法请求" }
                    });
                return;
            }

            // 验证是否为好友
            if (!await friendService.IsFriend(message.UserId, message.FriendId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new DeleteFriendMessage
                    {
                        Response = new CommonResponse { State = false, Message = "你们并非好友" }
                    });
                return;
            }

            int id = 0;
            DateTime time = DateTime.Now;
            try
            {
                // 删除relation，添加delete
                var friendRelationRepository = unitOfWork.GetRepository<FriendRelation>();

                // 查找并删除双向好友关系
                var relation1 = await friendRelationRepository.GetFirstOrDefaultAsync(
                    predicate: x => x.User1Id.Equals(message.UserId) && x.User2Id.Equals(message.FriendId));

                var relation2 = await friendRelationRepository.GetFirstOrDefaultAsync(
                    predicate: x => x.User1Id.Equals(message.FriendId) && x.User2Id.Equals(message.UserId));

                if (relation1 != null)
                    friendRelationRepository.Delete(relation1);

                if (relation2 != null)
                    friendRelationRepository.Delete(relation2);

                // 添加删除记录到FriendDelete表
                var friendDeleteRepository = unitOfWork.GetRepository<FriendDelete>();
                var friendDelete = new FriendDelete
                {
                    UserId1 = message.UserId,
                    UserId2 = message.FriendId,
                    Time = time
                };

                await friendDeleteRepository.InsertAsync(friendDelete);
                await unitOfWork.SaveChangesAsync();
                friendDeleteRepository.ChangeEntityState(friendDelete, Microsoft.EntityFrameworkCore.EntityState.Detached);
                id = friendDelete.Id;
            }
            catch
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new DeleteFriendMessage
                    {
                        Response = new CommonResponse { State = false, Message = "删除好友失败，数据库错误" }
                    });
                return;
            }

            // 向发起删除的用户发送成功响应
            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new DeleteFriendMessage
                {
                    Response = new CommonResponse { State = true, Message = "删除好友成功" },
                    UserId = message.UserId,
                    FriendId = message.FriendId,
                    DeleteId = id,
                    Time = time.ToInvariantString()
                });

            var targetChannel = clientChannelManager.GetClient(message.FriendId);
            if (targetChannel != null)
            {
                await targetChannel.WriteAndFlushProtobufAsync(new DeleteFriendMessage
                {
                    Response = new CommonResponse { State = true, Message = "您的好友关系已被对方删除" },
                    UserId = message.UserId,
                    FriendId = message.FriendId,
                    DeleteId = id,
                    Time = time.ToInvariantString()
                });
            }
        }
    }
}
