using Serilog;
using Microsoft.Extensions.Configuration;
using SocketServer.Server;
using Microsoft.Extensions.DependencyInjection;

namespace SocketServer
{
    public abstract class Application
    {
        /// <summary>
        /// IOC容器
        /// </summary>
        private IServiceProvider services;
        protected IServiceProvider Services
        {
            get { return services; }
        }

        /// <summary>
        /// 配置
        /// </summary>
        private IConfigurationRoot configuration;
        protected IConfigurationRoot Configuration
        {
            get { return configuration; }
        }

        protected ISocketServer SocketServer => services.GetService<ISocketServer>()!;

        public Application()
        { 
            // 配置初始化
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            InitConfigurations(configurationBuilder);
            configuration = configurationBuilder.Build();

            // 日志初始化
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            // IOC容器初始化
            IServiceCollection builder = new ServiceCollection();
            RegisterServices(builder);
            services = builder.BuildServiceProvider();
        }

        /// <summary>
        /// IOC容器注册服务
        /// </summary>
        /// <param name="registrator"></param>
        private void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton(configuration);
            services.AddSingleton(Log.Logger);
            services.AddSingleton<ISocketServer,Server.SocketServer>();
            RegisterServicesExtens(services);
        }

        /// <summary>
        /// 程序服务初始化
        /// </summary>
        /// <param name="registrator"></param>
        protected virtual void RegisterServicesExtens(IServiceCollection services) { }
    
        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="configurationBuilder"></param>
        protected virtual void InitConfigurations(IConfigurationBuilder configurationBuilder)
        { 
            configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        }

        /// <summary>
        /// 添加Socket处理程序
        /// </summary>
        /// <param name="builder"></param>
        protected abstract void ChannelHandler(SocketServerBuilder builder);

        /// <summary>
        /// 启动服务器
        /// </summary>
        public async Task Start()
        {
            OnStart();

            // 创建Socket服务,添加处理程序
            SocketServerBuilder builder = new SocketServerBuilder();
            ChannelHandler(builder);
           
            // 配置ChannelHandler
            SocketServer.Init(builder);

            // 启动服务器
            await SocketServer.Start();
        }

        protected virtual void OnStart() { }
    
        /// <summary>
        /// 关闭服务器
        /// </summary>
        public async Task Close() => await SocketServer.Stop();
    }
}
