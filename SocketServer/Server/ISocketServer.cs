using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Server
{
    public interface ISocketServer
    {
        Task Start();
        Task Stop();
        void Init(SocketServerBuilder builder);
    }
}
