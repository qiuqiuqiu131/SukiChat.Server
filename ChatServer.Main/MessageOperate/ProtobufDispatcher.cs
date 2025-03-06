using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatServer.Main.Entity;
using ChatServer.Common.Tool;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Serilog;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Reflection;

namespace ChatServer.Main.MessageOperate
{
    public delegate void MessageHandler<T>(MessageUnit<T> unit) where T : IMessage;

    public interface IProtobufDispatcher
    {
        void SendMessage(IChannel channel, IMessage message);
    }

    public interface IProtobufRegister
    {
        // 注册消息
        void Registe<T>(MessageHandler<T> handler) where T : IMessage;

        // 取消注册
        void UnRegiste<T>(MessageHandler<T> handler) where T : IMessage;
    }

    /// <summary>
    /// Protobuf消息分发器
    /// 用于将输入的Byte[]数据流转成IMessage，包装成MessageUnit，分发给对应的业务服务器
    /// 业务服务器通过注册消息，由ProtobufDispatcher分发消息
    /// </summary>
    public class ProtobufDispatcher : IProtobufDispatcher, IProtobufRegister
    {
        // 消息处理器字典
        private ConcurrentDictionary<Type, Delegate> handlerDic = new ConcurrentDictionary<Type, Delegate>();

        // 服务
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger logger;

        public ProtobufDispatcher(IServiceProvider serviceProvider, ILogger logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        /// <summary>
        /// Step1：打包成MessageUnit，分发消息
        /// </summary>
        /// <param name="channel">连接，消息来源</param>
        /// <param name="data">字节流</param>
        public void SendMessage(IChannel channel, IMessage message)
        {
            Type type = message.GetType();

            try
            {
                // 创建MessageUnit<>
                var unitType = typeof(MessageUnit<>).MakeGenericType(type);
                var unit = Activator.CreateInstance(unitType, message, channel)!;

                // 检查并调用对应的消息处理器
                if (handlerDic.ContainsKey(type))
                {
                    var handler = handlerDic[type];
                    handler.DynamicInvoke(unit);
                }
            }
            catch (Exception ex)
            {
                logger.Error("ExecuteMessage error:" + ex.StackTrace);
            }
        }

        #region 注册消息

        public void Registe<T>(MessageHandler<T> handler) where T : IMessage
        {
            Type type = typeof(T);
            handlerDic.AddOrUpdate(type, handler, (key, existingHandler)
                => (MessageHandler<T>)existingHandler + handler);
        }

        public void UnRegiste<T>(MessageHandler<T> handler) where T : IMessage
        {
            Type type = typeof(T);
            if (handlerDic.TryGetValue(type, out var existingHandler))
            {
                var newHandler = (MessageHandler<T>)existingHandler - handler;
                if (newHandler == null)
                    handlerDic.TryRemove(type, out _);
                else
                    handlerDic[type] = newHandler;
            }
        }

        #endregion
    }
}
