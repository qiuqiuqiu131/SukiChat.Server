using ChatServer.Common.Protobuf;
using ChatServer.Main.ServerEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Server
{
    class SearchServer : BusinessServer
    {
        public SearchServer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override void RegisteMessages(MessagesContainer messages)
        {
            messages.AddMessage<SearchUserRequest>()
                .AddMessage<SearchGroupRequest>();
        }
    }
}
