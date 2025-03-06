using ChatServer.Common.Tool;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Common
{
    public static class ChannelExtensions
    {
        public static async Task WriteAndFlushProtobufAsync(this IChannel channel, IMessage message)
        {
            byte[] bytes = ProtobufHelper.Serialize(message);
            IByteBuffer buffer = Unpooled.CopiedBuffer(bytes);
            await channel.WriteAndFlushAsync(buffer);
        }
    }
}
