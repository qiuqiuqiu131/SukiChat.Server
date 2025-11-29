using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ChatServer.DataBase.DataBase
{
    public class ChatServerDbContextFactory : IDesignTimeDbContextFactory<ChatServerDbContext>
    {
        public ChatServerDbContext CreateDbContext(string[] args)
        {
            var connectionString = DbSettings.Configuration.GetConnectionString("ChatDbConnection");

            //var connectionString = "server=localhost;port=3306;uid=root;pwd=123456;database=chatServer";

            var serverVersion = ServerVersion.AutoDetect(connectionString);

            var optionsBuilder = new DbContextOptionsBuilder<ChatServerDbContext>();
            optionsBuilder.UseMySql(connectionString,serverVersion);

            return new ChatServerDbContext(optionsBuilder.Options);
        }
    }
}
