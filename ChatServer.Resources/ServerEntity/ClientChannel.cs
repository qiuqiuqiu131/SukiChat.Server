using DotNetty.Transport.Channels;

namespace ChatServer.Resources.Entity
{
    public class ClientChannel
    {
        public IChannel Channel { get; init; }

        public bool isLogined { get; set; }
        public string? userId { get; set; }
        public DateTime? loginTime { get; set; }
        
        
        public ClientChannel(IChannel channel)
        {
            Channel = channel;
        }
        
        public void Login(string Id)
        {
            isLogined = true;
            userId = Id;
            loginTime = DateTime.Now;
        }
        
        public void Logout()
        {
            isLogined = false;
            userId = null;
            loginTime = null;
        }
    }
}
