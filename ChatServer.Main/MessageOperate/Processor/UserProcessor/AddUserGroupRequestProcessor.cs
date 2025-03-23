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

namespace ChatServer.Main.MessageOperate.Processor.UserProcessor
{
    class AddUserGroupRequestProcessor : IProcessor<AddUserGroupRequest>
    {
        private readonly IUserService userService;
        private readonly IUnitOfWork unitOfWork;

        public AddUserGroupRequestProcessor(IUserService userService,IUnitOfWork unitOfWork)
        {
            this.userService = userService;
            this.unitOfWork = unitOfWork;
        }

        public async Task Process(MessageUnit<AddUserGroupRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);

            var message = unit.Message;

            if(!await userService.IsUserExist(message.UserGroup.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new AddUserGroupResponse { Response = new CommonResponse { State = false, Message = "非法身份" } });
                return;
            }

            if (message.UserGroup.GroupName.Equals("默认分组"))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new AddUserGroupResponse { Response = new CommonResponse { State = false, Message = "无法添加默认分组" } });
                return;
            }

            var userGroupRepository = unitOfWork.GetRepository<UserGroup>();
            var userGroup = await userGroupRepository.GetFirstOrDefaultAsync(
                predicate: d => d.UserId.Equals(message.UserGroup.UserId) && 
                d.GroupName.Equals(message.UserGroup.GroupName) && 
                d.GroupType == message.UserGroup.GroupType);
            if(userGroup != null)
            {
                if(channel != null)
                    await channel.WriteAndFlushProtobufAsync(new AddUserGroupResponse { Response = new CommonResponse { State = true } });
                return;
            }

            try
            {
                await userGroupRepository.InsertAsync(new UserGroup
                {
                    UserId = message.UserGroup.UserId,
                    GroupName = message.UserGroup.GroupName,
                    GroupType = message.UserGroup.GroupType
                });
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex) 
            { 
                if(channel != null)
                    await channel.WriteAndFlushProtobufAsync(new AddUserGroupResponse { Response = new CommonResponse { State = false, Message = "服务器出错" } });
                return;
            }

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new AddUserGroupResponse
                {
                    Response = new CommonResponse { State = true }
                });
        }
    }
}
