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

namespace ChatServer.Main.MessageOperate.Processor.SearchProcessor
{
    class SearchGroupRequestProcessor : IProcessor<SearchGroupRequest>
    {
        private readonly IUserService userService;
        private readonly IUnitOfWork unitOfWork;

        public SearchGroupRequestProcessor(IUserService userService,IUnitOfWork unitOfWork)
        {
            this.userService = userService;
            this.unitOfWork = unitOfWork;
        }

        public async Task Process(MessageUnit<SearchGroupRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);

            var message = unit.Message;

            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new SearchGroupResponse { Response = new CommonResponse { State = false } });
                return;
            }

            var groupRepository = unitOfWork.GetRepository<Group>();
            var ids = await groupRepository.GetAll()
                .Where(d => (d.Name.Contains(message.Content) && message.Content.Length >= 3
                    || d.Name.Equals(message.Content)
                    || d.Description != null && (d.Description.Contains(message.Content) && message.Content.Length >=3 || d.Description.Equals(message.Content))
                    || d.Id.Equals(message.Content)) && d.IsDisband == false)
                .Select(d => d.Id).ToListAsync();

            var response = new SearchGroupResponse { Response = new CommonResponse { State = true } };
            response.Ids.AddRange(ids);

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(response);
        }
    }
}
