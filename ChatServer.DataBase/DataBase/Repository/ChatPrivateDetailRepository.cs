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
    class ChatPrivateDetailRepository : Repository<ChatPrivateDetail>
    {
        public ChatPrivateDetailRepository(ChatServerDbContext dbContext) : base(dbContext)
        {
        }
    }
}
