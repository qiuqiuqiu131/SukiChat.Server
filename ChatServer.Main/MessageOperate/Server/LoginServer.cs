using ChatServer.Common.Protobuf;
using ChatServer.Main.ServerEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Server
{
    public class LoginServer : BusinessServer
    {
        public LoginServer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override void RegisteMessages(MessagesContainer messages)
        {
            messages.AddMessage<RegisteRequest>()
                .AddMessage<LoginRequest>()
                .AddMessage<LogoutRequest>()
                .AddMessage<CreateGroupRequest>()
                .AddMessage<GetUserDetailMessageRequest>()
                .AddMessage<ResetPasswordRequest>()
                .AddMessage<ForgetPasswordRequest>()
                .AddMessage<OutlineMessageRequest>();
        }
    }
}
