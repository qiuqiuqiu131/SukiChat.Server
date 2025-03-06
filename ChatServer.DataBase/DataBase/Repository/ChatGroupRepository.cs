using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.Repository;

public class ChatGroupRepository : Repository<ChatGroup>
{
    public ChatGroupRepository(ChatServerDbContext dbContext) : base(dbContext)
    {
    }
}