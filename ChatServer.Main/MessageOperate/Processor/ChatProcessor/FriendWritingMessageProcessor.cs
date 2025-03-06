using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;

namespace ChatServer.Main.MessageOperate.Processor.ChatProcessor
{
    public class FriendWritingMessageProcessor : IProcessor<FriendWritingMessage>
    {
        private readonly IClientChannelManager channelManager;

        public FriendWritingMessageProcessor(IClientChannelManager channelManager)
        {
            this.channelManager = channelManager;
        }

        public async Task Process(MessageUnit<FriendWritingMessage> unit)
        {
            var client = channelManager.GetClient(unit.Message.UserTargetId);
            if (client == null) return;
            await client.WriteAndFlushProtobufAsync(unit.Message);
        }
    }
}
