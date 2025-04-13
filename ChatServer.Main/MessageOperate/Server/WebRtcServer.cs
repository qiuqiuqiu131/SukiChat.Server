using ChatServer.Common.Protobuf;
using ChatServer.Main.MessageOperate.Processor.WebRtcProcessor;
using ChatServer.Main.ServerEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Server
{
    class WebRtcServer : BusinessServer
    {
        public WebRtcServer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override void RegisteMessages(MessagesContainer messages)
        {
            messages.AddMessage<CallRequest>()
                .AddMessage<CallResponse>()
                .AddMessage<HangUp>()
                .AddMessage<VideoStateChanged>()
                .AddMessage<AudioStateChanged>()
                .AddMessage<SignalingMessage>();
        }
    }
}
