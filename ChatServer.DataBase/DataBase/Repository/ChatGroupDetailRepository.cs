using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.Repository
{
    class ChatGroupDetailRepository : Repository<ChatGroupDetail>
    {
        public ChatGroupDetailRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
