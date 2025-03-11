using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;

namespace ChatServer.Main.MessageOperate.Processor.RelationProcessor
{
    internal class UpdateFriendRelationRequestProcessor : IProcessor<UpdateFriendRelationRequest>
    {
        private readonly IUnitOfWork unitOfWork;

        public UpdateFriendRelationRequestProcessor(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task Process(MessageUnit<UpdateFriendRelationRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            var friendRelationRepository = unitOfWork.GetRepository<FriendRelation>();
            var friendRelation = await friendRelationRepository.GetFirstOrDefaultAsync(
                predicate: d => d.User1Id.Equals(message.UserId) && d.User2Id.Equals(message.FriendId),disableTracking:false);
            
            if(friendRelation == null)
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new UpdateFriendRelation
                    {
                        Response = new CommonResponse { State = false }
                    });
                return;
            }

            friendRelation.Remark = string.IsNullOrEmpty(message.Remark)?null:message.Remark;
            friendRelation.CantDisturb = message.CantDisturb;
            friendRelation.IsTop = message.IsTop;
            friendRelation.Grouping = message.Grouping;

            try
            {
                await unitOfWork.SaveChangesAsync();
            }
            catch
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new UpdateFriendRelation
                    {
                        Response = new CommonResponse { State = false }
                    });
                return;
            }

            if(channel != null)
                await channel.WriteAndFlushProtobufAsync(new UpdateFriendRelation
                {
                    Response = new CommonResponse { State = true },
                    UserId = message.UserId,
                    FriendId = message.FriendId
                });
        }
    }
}
