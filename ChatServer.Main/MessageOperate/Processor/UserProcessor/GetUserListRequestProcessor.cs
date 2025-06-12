using AutoMapper;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services;
using DotNetty.Transport.Channels;

namespace ChatServer.Main.MessageOperate.Processor.UserProcessor
{
    /// <summary>
    /// 批量获取用户好友的信息请求处理器
    /// </summary>
    class GetUserListRequestProcessor : IProcessor<GetUserListRequest>
    {
        private readonly IUserService userService;
        private readonly IFriendService friendService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IClientChannelManager clientChannelManager;
        private readonly IMapper mapper;

        public GetUserListRequestProcessor(IUserService userService, IFriendService friendService,
            IUnitOfWork unitOfWork, IClientChannelManager clientChannelManager,IMapper mapper)
        {
            this.userService = userService;
            this.friendService = friendService;
            this.unitOfWork = unitOfWork;
            this.clientChannelManager = clientChannelManager;
            this.mapper = mapper;
        }

        public async Task Process(MessageUnit<GetUserListRequest> unit)
        {
            unit.Channel.TryGetTarget(out IChannel? channel);
            var message = unit.Message;
        
            if(!await userService.IsUserExist(message.UserId))
            {
                if(channel != null)
                    await channel.WriteAndFlushProtobufAsync(new GetUserListResponse
                    {
                        Response = new CommonResponse { State = false, Message = "用户不存在" }
                    });
                return;
            }

            // 获取用户的所有好友ID
            var userList = await friendService.GetFriendsId(message.UserId);
            var userRepository = unitOfWork.GetRepository<User>();
            var users = await userRepository.GetPagedListAsync(
                predicate: d => userList.Contains(d.Id) || d.Id == message.UserId,
                orderBy: o => o.OrderBy(d => d.Id),
                pageIndex: message.PageIndex,
                pageSize: message.PageCount);

            // 并行处理用户信息，转为protobuf消息
            var list = users.Items.AsParallel().Select(user =>
            {
                var message = mapper.Map<UserMessage>(user);
                message.IsOnline = clientChannelManager.ClientOnline(user.Id);
                return message;
            }).ToList();

            var response = new GetUserListResponse
            {
                HasNext = users.HasNextPage,
                PageCount = users.PageSize,
                PageIndex = users.PageIndex,
                UserId = message.UserId,
                Response = new CommonResponse { State = true },
                Users = { list }
            };

            if(channel != null)
                await channel.WriteAndFlushProtobufAsync(response);
        }
    }
}
