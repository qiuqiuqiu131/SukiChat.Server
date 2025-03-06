using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Buffers;

namespace SocketServer.Server
{
    public class SocketServer : ISocketServer
    {
        private readonly IConfigurationRoot configuration;
        private readonly IServiceProvider services;
        private readonly ILogger logger;

        private IEventLoopGroup? bossGroup;
        private IEventLoopGroup? workerGroup;

        private List<Type>? channels;

        public SocketServer(IConfigurationRoot configuration, IServiceProvider services)
        {
            this.configuration = configuration;
            this.services = services;
            logger = services.GetService<ILogger>()!;
        }

        public void Init(SocketServerBuilder builder)
        {
            channels = builder.GetChannels();
        }

        public async Task Start() => await RunServerAsync();
        
        public async Task Stop()
        {
            if(workerGroup != null)
                await workerGroup.ShutdownGracefullyAsync();
            
            if(bossGroup != null)
                await bossGroup.ShutdownGracefullyAsync();
        }

        protected virtual async Task RunServerAsync()
        {
            try
            {
                // 设置环境变量,不记录已发字节流
                Environment.SetEnvironmentVariable("io.netty.allocator.numDirectArenas", "0");
                Environment.SetEnvironmentVariable("io.netty.allocator.numHeapArenas", "0");

                // 主工作线程组，设置为1个线程
                bossGroup = new MultithreadEventLoopGroup(1);
                // 工作线程组，默认为内核数*2的线程数
                workerGroup = new MultithreadEventLoopGroup();

                // 服务器引导程序
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoKeepalive,true)
                    .Option(ChannelOption.SoReuseport, true)
                    //.Option(ChannelOption.SoKeepalive, true)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;

                    // 默认功能
                    pipeline.AddLast("framing-enc", new LengthFieldPrepender(int.Parse(configuration["MaxFieldLength"]!)));
                    pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(
                        int.Parse(configuration["MaxFrameLength"]!),
                        0,int.Parse(configuration["MaxFieldLength"]!),
                        0,int.Parse(configuration["MaxFieldLength"]!)));
                    pipeline.AddLast(new IdleStateHandler(int.Parse(configuration["Heartbeat:ReadIdleTime"]!),0,0));

                    if (channels == null) return;
                    foreach (var type in channels)
                    {
                        IChannelHandler handle = (IChannelHandler)services.GetService(type)!;
                        pipeline.AddLast(handle);
                    }
                }));

                // 启动bootstrap
                IChannel boundChannel = await bootstrap.BindAsync(int.Parse(configuration["Address:Port"]!));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Server start error");
                await Stop();
            }
        }
    }
}
