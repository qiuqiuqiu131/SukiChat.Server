using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.Repository;

public class SacurityQuestionRepository : Repository<SacurityQuestion>
{
    public SacurityQuestionRepository(ChatServerDbContext dbContext) : base(dbContext) { }
}
