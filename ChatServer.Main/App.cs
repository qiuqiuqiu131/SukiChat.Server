using SocketServer;
using Microsoft.Extensions.Configuration;
using SocketServer.Server;
using ChatServer.Main.Services;
using Microsoft.Extensions.DependencyInjection;
using ChatServer.DataBase;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase;
using ChatServer.Main.MessageOperate;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.IOServer.ServerHandler;
using DotNetty.Handlers.Timeout;
using AutoMapper;
using ChatServer.Common;
using ChatServer.Main.Services.Helper;
using ChatServer.Main.Manager;

namespace ChatServer;

internal class App : Application
{
    protected override void InitConfigurations(IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddJsonFile("robotsettings.json", optional: true, reloadOnChange: true);
    }

    protected override void RegisterServicesExtens(IServiceCollection services)
    {
        #region IOServer
        // 客户端连接管理器
        services.AddSingleton<IClientChannelManager, ClientChannelManager>();

        // 服务器管道处理器
        services.AddTransient<EchoServerHandler>();
        services.AddTransient<ClientConnectHandler>();
        #endregion

        #region Helper
        services.AddTransient<ICipherHelper, CipherHelper>();
        #endregion

        #region Manager
        services.AddSingleton<IIdGeneratorManager, IdGeneratorManager>();
        #endregion

        #region Service
        services.AddTransient<ILoginService, LoginService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IFriendService, FriendService>();
        services.AddTransient<IGroupService, GroupService>();
        #endregion

        #region DataBase
        services.AddDataBaseServices();
        #endregion

        #region MessageOperate
        // protobuf 业务分发器
        services.AddSingleton<ProtobufDispatcher>();
        services.AddSingleton<IProtobufDispatcher>(p => p.GetRequiredService<ProtobufDispatcher>());
        services.AddSingleton<IProtobufRegister>(p => p.GetRequiredService<ProtobufDispatcher>());

        // 注入所有业务服务器
        services.AddBusinessServers();
        // 注入所有Processor处理器
        services.AddProcessors();
        #endregion

        #region Map
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DataToProtoProfile>();
        }).CreateMapper();
        services.AddSingleton(mapperConfig);
        #endregion
    }

    protected override void ChannelHandler(SocketServerBuilder builder)
    {
        builder.AddHandler<ClientConnectHandler>();
        builder.AddHandler<EchoServerHandler>();
    }

    protected override void OnStart()
    {
        Services.GetRequiredService<IIdGeneratorManager>();
        Services.GetRequiredService<IProtobufDispatcher>();

        foreach(var server in Services.GetServices<IBusinessServer>())
            server.Start();

        var logger = Services.GetService<ILogger>();
        logger?.Information("Main Server Started");
    }
}
