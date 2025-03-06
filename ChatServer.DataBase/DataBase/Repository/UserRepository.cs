using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.DataBase.UnitOfWork;

namespace ChatServer.DataBase.DataBase.Repository;

public class UserRepository : Repository<User>
{
    public UserRepository(ChatServerDbContext dbContext) : base(dbContext)
    {
    }
}

