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
    class RenameUserGroupRequestProcessor : IProcessor<RenameUserGroupRequest>
    {
        private readonly IUserService userService;
        private readonly IUnitOfWork unitOfWork;

        public RenameUserGroupRequestProcessor(IUserService userService,IUnitOfWork unitOfWork)
        {
            this.userService = userService;
            this.unitOfWork = unitOfWork;
        }

        public async Task Process(MessageUnit<RenameUserGroupRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);

            var message = unit.Message;

            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new RenameUserGroupResponse { Response = new CommonResponse { State = false, Message = "非法身份" } });
                return;
            }

            if(message.UserGroup.GroupName.Equals("默认分组"))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new RenameUserGroupResponse { Response = new CommonResponse { State = false, Message = "无法更改默认分组" } });
                return;
            }

            var userGroupRepository = unitOfWork.GetRepository<UserGroup>();
            var userGroup = await userGroupRepository.GetFirstOrDefaultAsync(
                predicate: d => d.UserId.Equals(message.UserId) && 
                d.GroupName.Equals(message.UserGroup.GroupName) && 
                d.GroupType == message.UserGroup.GroupType,
                disableTracking:false);
            if (userGroup == null)
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new RenameUserGroupResponse { Response = new CommonResponse { State = true, Message = "已经被删除了" } });
                return;
            }

            try
            {
                userGroup.GroupName = message.NewGroupName;

                // 将此分组的关系设置为默认分组中
                // 将此分组的关系设置为默认分组中
                if (message.UserGroup.GroupType == 0)
                {
                    var friendRelationRepository = unitOfWork.GetRepository<FriendRelation>();
                    var friendRelations = await friendRelationRepository.GetAllAsync(predicate: d => d.User1Id.Equals(message.UserGroup.UserId) && d.Grouping.Equals(message.UserGroup.GroupName), disableTracking: false);

                    foreach (var friendRelation in friendRelations)
                        friendRelation.Grouping = message.NewGroupName;
                }
                else
                {
                    var groupRelationRepository = unitOfWork.GetRepository<GroupRelation>();
                    var groupRelations = await groupRelationRepository.GetAllAsync(predicate: d => d.UserId.Equals(message.UserGroup.UserId) && d.Grouping.Equals(message.UserGroup.GroupName), disableTracking: false);

                    foreach (var groupRelation in groupRelations)
                        groupRelation.Grouping = message.NewGroupName;
                }

                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new RenameUserGroupResponse { Response = new CommonResponse { State = false, Message = "服务器出错" } });
                return;
            }

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new RenameUserGroupResponse { Response = new CommonResponse { State = true } });
        }
    }
}
