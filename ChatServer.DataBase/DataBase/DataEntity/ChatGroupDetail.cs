using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.DataEntity
{
    [PrimaryKey(nameof(UserId),nameof(ChatGroupId))]
    public class ChatGroupDetail
    {
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public User User { get; set; }

        [ForeignKey(nameof(ChatGroup))]
        public int ChatGroupId { get; set; }
        public ChatGroup ChatGroup { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime Time { get; set; }
    }
}
