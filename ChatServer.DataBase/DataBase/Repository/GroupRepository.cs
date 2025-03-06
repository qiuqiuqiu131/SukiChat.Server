using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;

namespace ChatServer.DataBase.DataBase.Repository;

public class GroupRepository : Repository<Group>
{
    public GroupRepository(ChatServerDbContext dbContext) : base(dbContext)
    {
    }
}