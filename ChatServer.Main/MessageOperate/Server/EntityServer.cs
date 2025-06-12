using ChatServer.Common.Protobuf;
using ChatServer.Main.ServerEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Server
{
    internal class EntityServer : BusinessServer
    {
        public EntityServer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override void RegisteMessages(MessagesContainer messages)
        {
            messages.AddMessage<GetUserRequest>()
                .AddMessage<GetUserListRequest>()
                .AddMessage<GroupMessageRequest>()
                .AddMessage<GroupMessageListRequest>()
                .AddMessage<GroupMemberListRequest>()
                .AddMessage<GroupMemberRequest>()
                .AddMessage<GroupMemberIdsRequest>()
                .AddMessage<ResetHeadImageRequest>()
                // UserGroup
                .AddMessage<DeleteUserGroupRequest>()
                .AddMessage<RenameUserGroupRequest>()
                .AddMessage<AddUserGroupRequest>()
                // 实体更新
                .AddMessage<UpdateUserDataRequest>()
                .AddMessage<UpdateGroupMessageRequest>()
                .AddMessage<UpdateFriendRelationRequest>()
                .AddMessage<UpdateGroupRelationRequest>()
                // 删除关系
                .AddMessage<DeleteFriendRequest>()
                .AddMessage<QuitGroupRequest>()
                .AddMessage<DisbandGroupRequest>()
                .AddMessage<RemoveMemberRequest>();
        }
    }
}
