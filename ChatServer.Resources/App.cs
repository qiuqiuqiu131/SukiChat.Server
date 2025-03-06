using ChatServer.Resources.IOServer.Manager;
using ChatServer.Resources.IOServer.ServerHandler;
using ChatServer.Resources.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SocketServer;
using SocketServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Resources
{
    internal class App : Application
    {
        protected override void InitConfigurations(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        }

        protected override void RegisterServicesExtens(IServiceCollection services)
        {
            services.AddSingleton<IClientChannelManager, ClientChannelManager>();

            services.AddTransient<ClientConnectHandler>();
            services.AddTransient<ResourcesServerHandler>();

            services.AddTransient<FileOperator>();

        }

        protected override void ChannelHandler(SocketServerBuilder builder)
        {
            builder.AddHandler<ClientConnectHandler>();
            builder.AddHandler<ResourcesServerHandler>();
        }

        protected override void OnStart()
        {
            var logger = Services.GetService<ILogger>();
            logger?.Information("Resources Server Started");
        }
    }
}
