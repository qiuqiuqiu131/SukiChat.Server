using ChatServer.Main.Entity;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate
{
    public interface IProcessor<T> where T : IMessage
    {
        Task Process(MessageUnit<T> unit);
    }
}
