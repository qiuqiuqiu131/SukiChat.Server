using AutoMapper;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;

namespace ChatServer.Main.MessageOperate.Processor.GroupProcessor;

class GroupMessageListRequestProcessor : IProcessor<GroupMessageListRequest>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IUserService userService;
    private readonly IGroupService groupService;
    private readonly IMapper mapper;

    public GroupMessageListRequestProcessor(IUnitOfWork unitOfWork, IUserService userService,
        IGroupService groupService,IMapper mapper)
    {
        this.unitOfWork = unitOfWork;
        this.userService = userService;
        this.groupService = groupService;
        this.mapper = mapper;
    }

    public async Task Process(MessageUnit<GroupMessageListRequest> unit)
    {
        unit.Channel.TryGetTarget(out var channel);
        var message = unit.Message;

        if(!await userService.IsUserExist(message.UserId))
        {
            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new GroupMessageListResponse
                {
                    Response = new CommonResponse { State = false, Message = "用户不存在" }
                });
            return;
        }

        // 获取群聊列表
        var groups = await groupService.GetGroupsOfUser(message.UserId);

        var groupRepository = unitOfWork.GetRepository<Group>();
        var entitys = await groupRepository.GetPagedListAsync(
            predicate: d => groups.Contains(d.Id),
            orderBy: o => o.OrderBy(d => d.Id),
            pageSize: message.PageCount,
            pageIndex: message.PageIndex);

        var lists = entitys.Items.AsParallel().Select(d =>
        {
            var message = mapper.Map<GroupMessage>(d);
            return message;
        });

        var response = new GroupMessageListResponse
        {
            Response = new CommonResponse { State = true },
            PageCount = entitys.PageSize,
            PageIndex = entitys.PageIndex,
            HasNext = entitys.HasNextPage,
            UserId = message.UserId,
            Groups = { lists }
        };

        if (channel != null)
            await channel.WriteAndFlushProtobufAsync(response);
    }
}
