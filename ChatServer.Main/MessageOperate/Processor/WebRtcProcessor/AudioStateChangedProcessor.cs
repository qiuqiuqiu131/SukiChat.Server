using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.WebRtcProcessor
{
    class AudioStateChangedProcessor : IProcessor<AudioStateChanged>
    {
        private readonly IClientChannelManager clientChannelManager;

        public AudioStateChangedProcessor(IClientChannelManager clientChannelManager)
        {
            this.clientChannelManager = clientChannelManager;
        }

        public async Task Process(MessageUnit<AudioStateChanged> unit)
        {
            var client = clientChannelManager.GetClient(unit.Message.TargetId);
            if (client != null)
                await client.WriteAndFlushProtobufAsync(unit.Message);
        }
    }
}
