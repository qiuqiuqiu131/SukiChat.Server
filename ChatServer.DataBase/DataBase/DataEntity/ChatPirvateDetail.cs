using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.DataEntity
{
    [PrimaryKey(nameof(UserId), nameof(ChatPrivateId))]
    public class ChatPrivateDetail
    {
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public User User { get; set; }

        [ForeignKey(nameof(ChatPrivate))]
        public int ChatPrivateId { get; set; }
        public ChatPrivate ChatPrivate { get; set; }

        public bool IsDeleted { get; set; }
    
        public DateTime Time { get; set; }
    }
}
