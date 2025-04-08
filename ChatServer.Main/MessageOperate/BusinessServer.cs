using ChatServer.Main.Entity;
using ChatServer.Main.ServerEntity;
using DotNetty.Common.Utilities;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Metadata;

namespace ChatServer.Main.MessageOperate
{
    public interface IBusinessServer
    {
        void Start();
    }

    /// <summary>
    /// BusinessServer基类业务服务器
    /// 消息队列处理 BlockingCollection<object>
    /// 接受从ProtobufDispatcher中接受消息，将消息放入消息队列
    /// 一个业务服务器可以注册多个消息处理器
    /// 
    /// 实际上，是对Protobuf消息进行分类
    /// </summary>
    public abstract class BusinessServer : IBusinessServer
    {
        // 消息队列
        private readonly BlockingCollection<object> queue = new BlockingCollection<object>();
        // 信号量,控制消息队列大小
        private readonly SemaphoreSlim semaphore;

        // 消息容器
        private readonly MessagesContainer messages;
        private List<Type> Messages => messages.Types;

        // 服务
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;

        public BusinessServer(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetService<ILogger>()!;
            this.serviceProvider = serviceProvider;

            messages = new MessagesContainer();
            RegisteMessages(messages);
            RegisteMessage();

            semaphore = new SemaphoreSlim(GetQueueSize());
        }

        /// <summary>
        /// 业务服务器启动
        /// </summary>
        public void Start()
        {
            // 启动消费者任务
            Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 读取配置，同步线程数量
        /// </summary>
        /// <returns></returns>
        private int GetQueueSize()
        {
            IConfigurationRoot configuration = serviceProvider.GetService<IConfigurationRoot>()!;
            string targetString = $"Server:{GetType().Name}:QueueSize";
            int size = 50;

            try
            {
                string? value = configuration[targetString];
                if (value != null)
                    size = int.Parse(value);
            }
            catch { }

            return size;
        }

        #region Message Register

        /// <summary>
        /// 将想要监听的消息注册到消息容器中
        /// </summary>
        /// <param name="messages">消息容器</param>
        protected abstract void RegisteMessages(MessagesContainer messages);

        /// <summary>
        /// 将消息容器中的消息注册到ProtobufDispatcher
        /// </summary>
        private void RegisteMessage()
        {
            IProtobufRegister register = serviceProvider.GetService<IProtobufRegister>()!;

            try
            {
                MethodInfo registMethod = register.GetType().GetMethod("Registe")!;
                MethodInfo enqueueMethod = GetType().GetMethod("Enqueue", BindingFlags.NonPublic | BindingFlags.Instance)!;

                foreach (var type in Messages)
                {
                    if (typeof(IMessage).IsAssignableFrom(type))
                    {
                        // 创建注册方法泛型
                        MethodInfo method = registMethod.MakeGenericMethod(type);
                        MethodInfo enqueueGenericMethod = enqueueMethod.MakeGenericMethod(type);
                        Delegate handler = Delegate.CreateDelegate(typeof(MessageHandler<>).MakeGenericType(type), this, enqueueGenericMethod);
                        method.Invoke(register, [handler]);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error registering message: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// 接受消息单元，放入消息队列
        /// </summary>
        /// <typeparam name="T">IMessage泛型</typeparam>
        /// <param name="messageUnit">消息单元</param>
        protected void Enqueue<T>(MessageUnit<T> messageUnit) where T : IMessage
            => queue.Add(messageUnit);
        #endregion

        #region Process
        /// <summary>
        /// 控制消息队列的执行
        /// </summary>
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
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            }
        }

        /// <summary>
        /// 处理消息单元MessageUnit<T> ,将其分发给对应的处理器processor
        /// </summary>
        /// <param name="obj"></param>
        private async Task OperateMessageUnit(object obj)
        {
            // 获取obj的属性Message
            PropertyInfo? messageProperty = obj.GetType().GetProperty("Message");

            if(messageProperty == null)
                throw new Exception("MessageUnit<T> does not have a property named Message");

            // 获取消息处理器，根据IMessage类型获取对应的处理器
            Type type = messageProperty.PropertyType;
            Type? processorType = typeof(IProcessor<>).MakeGenericType(type);

            // 创建一个作用域，生成处理消息的具体业务逻辑单元
            using (var scope = serviceProvider.CreateScope())
            {
                IServiceProvider scopeProvider = scope.ServiceProvider;

                // 获取处理器实例，一个Protobuf消息对应多个处理器
                var processors = scopeProvider.GetServices(processorType)!;

                // 获取处理方法
                MethodInfo? processMethod = processorType.GetMethod("Process");

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
        #endregion
    }
}
