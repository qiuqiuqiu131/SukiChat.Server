using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.DataEntity
{
    public class UserOnline
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // 上线时间
        [Required]
        public DateTime LoginTime { get; set; }

        // 下线时间
        [Required]
        public DateTime LogoutTime { get; set; }

        [Required]
        [StringLength(10)]
        [ForeignKey(nameof(User))]
        public string UserId {  get; set; }

        [Required]
        public User User { get; set; }
    }
}
