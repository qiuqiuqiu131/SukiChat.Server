using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.DataEntity
{
    public class FriendRelation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(User1))]
        [StringLength(10)]
        public string User1Id { get; set; }
        public User User1 { get; set; }

        [ForeignKey(nameof(User2))]
        [StringLength(10)]
        public string User2Id { get; set; }
        public User User2 { get; set; }

        [Required]
        [StringLength(20)]
        public string Grouping { get; set; }

        [Required]
        public DateTime GroupTime { get; set; }

        [StringLength(30)]
        public string? Remark { get; set; }

        [Required]
        public bool CantDisturb { get; set; } = false;

        [Required]
        public bool IsTop { get; set; } = false;

        [Required]
        public int LastChatId { get; set; } = 0;
    }
}
