# Suki Chat聊天服务器

这是一个基于DotNetty实现的高性能Socket聊天服务器，使用Protobuf进行高效的数据序列化和通信。该项目展示了如何构建一个可扩展的消息处理框架，并集成了依赖注入容器进行组件管理。

## 技术栈

- **DotNetty**: 高性能网络应用框架，处理底层Socket通信
- **Protocol Buffers (Protobuf)**: 用于数据序列化和结构化的通信协议
- **依赖注入**: 使用Microsoft.Extensions.DependencyInjection实现IoC容器
- **Entity Framework Core**: 处理数据库访问和ORM映射
- **Serilog**: 用于日志记录

## 项目架构

项目采用模块化架构，各组件职责分明：

### 核心组件

- **SocketServer**: 基础Socket服务器框架，负责连接管理和消息传输
- **ChatServer.Main**: 包含业务逻辑和消息处理
- **ChatServer.Common**: 共享的工具类和Protobuf定义
- **ChatServer.DataBase**: 数据库访问层

## 框架设计

### 应用启动与服务注册

整个应用程序基于`Application`抽象类构建，提供统一的启动流程：

```csharp
public abstract class Application
{
    // 初始化配置和IOC容器
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
    
    // 启动服务器
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
    
    // ...其他代码
}
```

### DotNetty与依赖注入集成

项目通过精心设计将DotNetty框架与Microsoft.Extensions.DependencyInjection依赖注入容器深度集成：

1. **注册ChannelHandler**：
```csharp
// 在App.cs中注册
services.AddTransient<EchoServerHandler>();
services.AddTransient<ClientConnectHandler>();
```

2. **声明Pipeline处理链**：
```csharp
// 在App.cs中声明处理链顺序
protected override void ChannelHandler(SocketServerBuilder builder)
{
    builder.AddHandler<ClientConnectHandler>();
    builder.AddHandler<EchoServerHandler>();
}
```

3. **从容器创建Handler实例**：
```csharp
// 在SocketServer中，从IOC容器获取Handler实例
foreach (var type in channels)
{
    IChannelHandler handle = (IChannelHandler)services.GetService(type)!;
    pipeline.AddLast(handle);
}
```

### 自动组件发现与注册

系统使用反射机制自动发现和注册组件，减少手动配置：

```csharp
// 自动注册所有处理器
public static void AddProcessors(this IServiceCollection services)
{
    var processorType = typeof(IProcessor<>);
    var types = Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => t.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == processorType)
            && t.IsClass && !t.IsAbstract).ToList();

    foreach (var type in types)
    {
        var interfaceType = type.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == processorType);
        services.AddScoped(interfaceType, type);
    }
}
```

## 消息处理流程

系统采用多级消息处理架构，确保高效、可靠的消息处理：

### 1. 消息接收阶段
当客户端发送消息到服务器时：
- 首先经过DotNetty的`LengthFieldBasedFrameDecoder`解决TCP粘包问题
- 消息被`ClientConnectHandler`接收并转换为字节数组
- 如果是心跳包消息，直接在Handler层处理
- 其他消息通过`ChannelRead`方法传递到下一个Handler

```csharp
// 在ClientConnectHandler中接收原始数据
public override void ChannelRead(IChannelHandlerContext context, object message)
{
    var buffer = message as IByteBuffer;
    if (buffer == null) return;

    var readableBytes = new byte[buffer.ReadableBytes];
    buffer.GetBytes(buffer.ReaderIndex, readableBytes);
    buffer.Release();

    // 解析为Protobuf消息
    IMessage mess = ProtobufHelper.ParseFrom(readableBytes);

    // 心跳包处理
    if (mess is HeartBeat)
    {
        readIdleTimes = 0;
        return;
    }
    
    // 传递给下一个Handler
    base.ChannelRead(context, mess);
}
```

### 2. 消息分发阶段
消息到达`EchoServerHandler`后：
- 调用`ProtobufDispatcher.SendMessage`方法分发消息
- `ProtobufDispatcher`创建对应类型的`MessageUnit<T>`
- 查找已注册的处理该类型消息的Handler并调用

```csharp
// EchoServerHandler处理业务消息
protected override void ChannelRead0(IChannelHandlerContext context, IMessage message)
{
    if (message != null)
    {
        // 通过ProtobufDispatcher分发消息
        dispatcher.SendMessage(context.Channel, message);
    }
}
```

### 3. 消息队列处理阶段
消息被分发到订阅的BusinessServer：
- BusinessServer在启动时通过`RegisteMessage`方法订阅特定类型的消息
- 收到消息后，BusinessServer将其加入`BlockingCollection<object>`队列
- 后台线程控制消息处理速度，防止系统过载

```csharp
// BusinessServer将消息放入队列
protected void Enqueue<T>(MessageUnit<T> messageUnit) where T : IMessage
    => queue.Add(messageUnit);

// 消息队列处理方法
private void ProcessQueue()
{
    foreach (var unit in queue.GetConsumingEnumerable())
    {
        semaphore.Wait();
        Task.Run(async () =>
        {
            try
            {
                await OperateMessageUnit(unit);
            }
            finally
            {
                semaphore.Release();
            }
        });
    }
}
```

### 4. 消息处理阶段
从队列取出消息后：
- 创建作用域并获取对应消息类型的所有`IProcessor<T>`实例
- 依次调用每个Processor的Process方法处理消息
- Processor实现具体业务逻辑，如数据库操作、状态更新等

```csharp
// BusinessServer处理消息
private async Task OperateMessageUnit(object obj)
{
    // ...类型识别代码...

    // 创建一个作用域，生成处理消息的具体业务逻辑单元
    using (var scope = serviceProvider.CreateScope())
    {
        IServiceProvider scopeProvider = scope.ServiceProvider;

        // 获取处理器实例，一个Protobuf消息对应多个处理器
        var processors = scopeProvider.GetServices(processorType)!;

        // 触发处理方法
        foreach (var processor in processors)
        {
            try
            {
                // 调用处理器
                if (processMethod != null)
                {
                    var task = (Task)processMethod.Invoke(processor, [obj])!;
                    await task;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
    }
}
```

### 处理器实现示例

```csharp
// 处理器示例实现
public class LoginRequestProcessor : IProcessor<LoginRequest>
{
    private readonly ILoginService loginService;
    
    public LoginRequestProcessor(ILoginService loginService)
    {
        this.loginService = loginService;
    }
    
    public async Task Process(MessageUnit<LoginRequest> unit)
    {
        LoginRequest request = unit.Message;
        IChannel channel = unit.Channel;
        
        // 验证登录信息
        var result = await loginService.ValidateLogin(request.UserName, request.Password);
        
        // 生成响应并发送回客户端
        LoginResponse response = new LoginResponse 
        {
            Success = result.Success,
            Message = result.Message
        };
        
        await channel.WriteAndFlushProtobufAsync(response);
    }
}
```

## 资源服务器(ChatServer.Resources)

除了主服务器外，系统还包含一个专门的资源服务器模块(ChatServer.Resources)，用于高效处理文件上传和下载。

### 文件传输架构

资源服务器采用分片传输机制，确保大文件的可靠传输：

```csharp
// 文件操作工具类处理分片传输
internal class FileOperator
{
    // ...文件单元字典
    private Dictionary<string, FileUnit> FileUnitDicts = new();
    // 文件接收完成事件
    public event Action<(bool, string)> OnFileAllReceived;
    // 分片大小常量
    const int CHUNK_SIZE = ushort.MaxValue; // 64KB per chunk
    
    // ...各种文件操作方法
}
```

### 文件传输流程

1. **文件请求阶段**：客户端发送`FileRequest`请求文件
2. **文件头响应**：服务器返回`FileHeader`包含文件元数据
3. **分片传输阶段**：
   - 发送方将文件分割为多个`FilePack`
   - 接收方处理每个分片并回复`FilePackResponse`
4. **完成确认阶段**：全部分片接收完毕后发送`FileResponse`

```csharp
// ResourcesServerHandler处理文件消息
protected override async void ChannelRead0(IChannelHandlerContext context, IMessage message)
{
    if(message.GetType() == typeof(FileHeader))
    {
        // 处理文件头
        var mess = (FileHeader) message;
        await _fileOperator.ReceiveFileHeader(mess);
        // ...发送响应
    }
    else if(message.GetType() == typeof(FilePack))
    {
        // 处理文件分片
        var response = _fileOperator.ReceiveFilePack((FilePack)message);
        // ...发送响应
    }
    // ...处理其他消息类型
}
```

### 文件存储与状态管理

资源服务器使用`FileUnit`类跟踪每个文件的传输状态：

```csharp
internal class FileUnit
{
    public string Path { get; set; }
    public string FileName { get; set; }
    public string Type { get; set; }
    public int CurrentIndex { get; set; }  // 当前处理的分片索引
    public int TotleSize { get; set; }     // 文件总大小
    public int TotleCount { get; set; }    // 分片总数
    public string Time { get; set; }       // 时间戳，作为文件传输会话的唯一标识
    public FileStream? fileStream { get; set; } // 文件流
    
    // ...构造函数和其他成员
}
```

### 文件传输优化

1. **分片校验**：每个分片包含索引和大小信息，确保按顺序接收
2. **断点续传**：通过分片索引支持断点续传
3. **资源清理**：完成传输或出错时自动关闭文件流和清理临时资源
4. **异步处理**：所有I/O操作都使用异步方法，防止阻塞网络线程

### 与主服务器集成

资源服务器共享相同的框架设计，使用相同的依赖注入容器和消息处理机制：

```csharp
internal class App : Application
{
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
    
    // ...其他方法
}
```

资源服务器的这种设计确保了大文件传输的可靠性和高效性，同时保持了与主服务器架构的一致性。

## 系统的核心优势

### 松耦合架构

- **组件隔离**: 系统各组件通过接口通信，降低了模块间的耦合
- **可测试性**: 每个组件都可以独立测试，支持单元测试和集成测试
- **灵活扩展**: 可以方便地添加新的消息类型和处理器，无需修改框架代码

### 高性能设计

- **高效序列化**: 使用Protobuf进行高效的消息序列化和反序列化
- **消息队列缓冲**: 使用BlockingCollection控制消息处理速度
- **连接池管理**: 高效管理客户端连接资源
- **异步处理**: 全链路异步操作，提高系统吞吐量

### 可扩展性

- **SocketServerBuilder**: 允许动态添加ChannelHandler
- **MessageContainer**: 支持动态注册需要监听的消息类型
- **自动注册**: 通过反射自动发现并注册组件
- **依赖注入**: 所有组件都从容器获取，方便替换实现

## 总结

通过DotNetty与依赖注入的无缝集成，结合Protobuf的高效序列化，实现了一个既灵活又高效的实时通信系统。清晰的简单分层架构确保系统易于理解和扩展，使开发者能够专注于业务逻辑而非底层通信细节。