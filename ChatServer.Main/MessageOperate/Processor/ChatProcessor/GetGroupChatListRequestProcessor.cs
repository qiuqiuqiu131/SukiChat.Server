using AutoMapper;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.ChatProcessor
{
    class GetGroupChatListRequestProcessor : IProcessor<GetGroupChatListRequest>
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IGroupService groupService;

        public GetGroupChatListRequestProcessor(IMapper mapper,IUnitOfWork unitOfWork, IUserService userService, IGroupService groupService)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.groupService = groupService;
        }

        public async Task Process(MessageUnit<GetGroupChatListRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;


        }
    }
}
