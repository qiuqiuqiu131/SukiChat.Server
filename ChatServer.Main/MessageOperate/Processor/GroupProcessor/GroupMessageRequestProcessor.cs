using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.GroupProcessor
{
    internal class GroupMessageRequestProcessor : IProcessor<GroupMessageRequest>
    {
        private readonly IUserService userService;
        private readonly IUnitOfWork unitOfWork;

        public GroupMessageRequestProcessor(IUserService userService,
            IUnitOfWork unitOfWork)
        {
            this.userService = userService;
            this.unitOfWork = unitOfWork;
        }

        public async Task Process(MessageUnit<GroupMessageRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            // 判断userId是否存在
            if(! await userService.IsUserExist(message.UserId))
            {
                if(channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new GroupMessage
                    {
                        Response = new CommonResponse { State = false }
                    });
                }
                return;
            }

            var groupRepository = unitOfWork.GetRepository<Group>();
            var group = await groupRepository.GetFirstOrDefaultAsync(predicate: d => d.Id.Equals(message.GroupId));

            if (group == null)
            {
                if(channel != null)
                    await channel.WriteAndFlushProtobufAsync(new GroupMessage { Response = new CommonResponse { State = false } });
                return;
            }

            GroupMessage response = new GroupMessage
            {
                Response = new CommonResponse { State = true },
                GroupId = message.GroupId,
                Description = group.Description ?? string.Empty,
                CreateTime = group.CreateTime.ToString(),
                HeadIndex = group.HeadIndex,
                Name = group.Name
            };

            if(channel != null)
                await channel.WriteAndFlushProtobufAsync(response);
        }
    }
}
