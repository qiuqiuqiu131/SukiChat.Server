using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.ServerEntity
{
    public class MessagesContainer
    {
        private List<Type> types = new List<Type>();
        public List<Type> Types => types;

        public MessagesContainer AddMessage<T>() where T : IMessage
        {
            Type type = typeof(T);
            if (!types.Contains(type))
                types.Add(type);

            return this;
        }

        public MessagesContainer AddMessage(Type type)
        {
            if (typeof(IMessage).IsAssignableFrom(type) && !types.Contains(type))
                types.Add(type);

            return this;
        }
    }
}
