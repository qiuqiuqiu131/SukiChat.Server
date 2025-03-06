
using ChatServer.Common.Protobuf;
using ChatServer.Common.Tool;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;

namespace ChatServer.Test
{
    public static class Program
    {
        static async Task RunClientAsync()
        {
            var group = new MultithreadEventLoopGroup();
            try
            {
                var bootstrap = new Bootstrap();
                bootstrap
                .Group(group)
                .Channel<TcpSocketChannel>()
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                        pipeline.AddLast(new EchoClientHandler());//client的channel的处理类实现
                    }));

                IChannel clientChannel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 13002));//设置服务端的端口号和ip地址

                RegisteRequest registeRequest = new RegisteRequest
                {
                    Name = "丘丘丘",
                    Password = "123456"
                };

                byte[] bytes = ProtobufHelper.Serialize(registeRequest);
                await clientChannel.WriteAndFlushAsync(Unpooled.CopiedBuffer(bytes));

                Console.ReadLine();

                await clientChannel.CloseAsync();
            }
            finally
            {
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            }
        }

        static void Main() => RunClientAsync().Wait();
    }
}