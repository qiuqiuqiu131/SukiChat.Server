using AutoMapper;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;

namespace ChatServer.Main.MessageOperate.Processor.ChatProcessor
{
    class FriendChatMessageGroupProcessor(IUnitOfWork unitOfWork, 
        IUserService userService, IFriendService friendService, IMapper mapper) : IProcessor<GetFriendChatListRequest>
    {
        public async Task Process(MessageUnit<GetFriendChatListRequest> unit)
        {
            var message = unit.Message;
            unit.Channel.TryGetTarget(out var channel);
            if (channel == null) return;
            
            if(await userService.IsUserExist(message.UserId))
            {
                await channel.WriteAndFlushProtobufAsync(new GetFriendChatListResponse
                {
                    Response = new CommonResponse { State = false, Message = "非法用户访问" }
                });
                return;
            }

            if(await friendService.IsFriend(message.UserId ,message.FriendId))
            {
                await channel.WriteAndFlushProtobufAsync(new GetFriendChatListResponse
                {
                    Response = new CommonResponse { State = false, Message = "你们不是好友关系" }
                });
                return;
            }

            DateTime lastLoginTime = DateTime.Parse(message.LastLoginTime);

            var repository = unitOfWork.GetRepository<ChatPrivate>();
            var res = await repository.GetPagedListAsync(
                predicate: d => ((d.UserFromId == message.UserId) && (d.UserTargetId == message.FriendId) || (d.UserFromId == message.FriendId) && (d.UserTargetId == message.UserId)) && (d.Time < lastLoginTime || d.RetractTime < lastLoginTime), 
                orderBy: d => d.OrderByDescending(o => o.Time),
                pageIndex:message.PageIndex,
                pageSize:message.PageCount);
            var response = new GetFriendChatListResponse
            {
                Response = new CommonResponse { State = true },
                UserId = message.UserId,
                FriendId = message.FriendId,
                PageIndex = res.PageIndex,
                PageCount = res.PageSize,
                HasNext = res.HasNextPage
            };
            response.Messages.AddRange(mapper.Map<List<FriendChatMessage>>(res.Items));
        
            await channel.WriteAndFlushProtobufAsync(response);
        }
    }
}
