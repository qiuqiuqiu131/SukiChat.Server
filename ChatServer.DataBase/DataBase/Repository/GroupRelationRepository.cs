using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;

namespace ChatServer.DataBase.DataBase.Repository;

public class GroupRelationRepository : Repository<GroupRelation>
{
    public GroupRelationRepository(ChatServerDbContext dbContext) : base(dbContext)
    {
    }
}