using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.DataBase.UnitOfWork;

namespace ChatServer.DataBase.DataBase.Repository;

public class UserOnlineRepository:Repository<UserOnline>
{
    public UserOnlineRepository(ChatServerDbContext dbContext) : base(dbContext)
    {
    }
}