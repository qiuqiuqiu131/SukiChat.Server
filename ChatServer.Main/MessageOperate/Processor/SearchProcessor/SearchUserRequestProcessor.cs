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
    class SearchUserRequestProcessor : IProcessor<SearchUserRequest>
    {
        private readonly IUserService userService;
        private readonly IUnitOfWork unitOfWork;

        public SearchUserRequestProcessor(IUserService userService,IUnitOfWork unitOfWork)
        {
            this.userService = userService;
            this.unitOfWork = unitOfWork;
        }

        public async Task Process(MessageUnit<SearchUserRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);

            var message = unit.Message;

            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new SearchUserResponse { Response = new CommonResponse { State = false } });
                return;
            }

            var userRepository = unitOfWork.GetRepository<User>();
            var ids = await userRepository.GetAll()
                .Where(d => d.Introduction != null && (d.Introduction.Contains(message.Content) && message.Content.Length >= 2 || d.Name.Equals(message.Content))
                    || d.Name.Equals(message.Content)
                    || d.Name.Contains(message.Content) && message.Content.Length >= 2
                    || d.Id.Equals(message.Content))
                .Select(d => d.Id).ToListAsync();

            var response = new SearchUserResponse{ Response = new CommonResponse { State = true }};
            response.Ids.AddRange(ids);

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(response);
        }
    }
}
