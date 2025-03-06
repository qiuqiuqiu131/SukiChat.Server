using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.Common.Tool;
using ChatServer.Resources.Entity;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.Resources.IOServer.Manager
{
    public interface IClientChannelManager
    {
        void AddClient(IChannel channel);
        void RemoveClient(IChannel? channel);
        bool ClientOnline(string userId);
        IChannel? GetClient(string userId);
    }

    /// <summary>
    /// 管理用户连接以及用户登录状态
    /// </summary>
    public class ClientChannelManager : IClientChannelManager
    {
        private readonly IServiceProvider serviceProvider;

        private List<ClientChannel> channels = [];

        public ClientChannelManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 连接建立时调用
        /// </summary>
        /// <param name="channel"></param>
        public void AddClient(IChannel channel)
        {
            int index = channels.FindIndex(c => Equals(c.Channel, channel));
            if (index == -1)
                channels.Add(new ClientChannel(channel));
        }

        /// <summary>
        /// 连接断开时调用
        /// </summary>
        /// <param name="channel"></param>
        public void RemoveClient(IChannel? channel)
        {
            if (channel == null) return;

            int index = channels.FindIndex(c => Equals(c.Channel, channel));
            if (index != -1)
                channels.RemoveAt(index);
        }

        public bool ClientOnline(string userId)
        {
            return channels.Exists(c => c.userId != null && c.userId.Equals(userId));
        }

        public IChannel? GetClient(string userId)
        {
            return channels.FirstOrDefault(c => c.userId != null && c.userId.Equals(userId))?.Channel;
        }
    }
}
