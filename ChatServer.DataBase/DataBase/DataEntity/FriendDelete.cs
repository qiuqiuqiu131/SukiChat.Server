using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.DataEntity
{
    public class FriendDelete
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(10)]
        [ForeignKey(nameof(User1))]
        public string UserId1 { get; set; }
        public User User1 { get; set; }

        [StringLength(10)]
        [ForeignKey(nameof(User2))]
        public string UserId2 { get; set; }
        public User User2 { get; set; }

        [Required]
        public DateTime Time { get; set; }
    }
}
