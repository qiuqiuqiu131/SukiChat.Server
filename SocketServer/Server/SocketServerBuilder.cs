using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Server
{
    public class SocketServerBuilder
    {
        List<Type> types = new List<Type>();

        public void AddHandler<T>() where T : IChannelHandler
        {
            types.Add(typeof(T));
        }

        public List<Type> GetChannels()
            => types;
    }
}
