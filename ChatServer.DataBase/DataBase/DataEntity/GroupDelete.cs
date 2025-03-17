using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.DataEntity
{
    public class GroupDelete
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Group))]
        [StringLength(10)]
        public string GroupId { get; set; }
        public Group Group { get; set; }

        [StringLength(10)]
        [ForeignKey(nameof(Member))]
        public string MemberId { get; set; }
        public User Member { get; set; }

        [Required]
        public int DeleteMethod { get; set; }
        // 0:退出群聊
        // 1:管理员移除成员
        // 2:群主解散群聊

        [StringLength(10)]
        [ForeignKey(nameof(OperateUser))]
        public string OperateUserId { get; set; }
        public User OperateUser { get; set; }

        [Required]
        public DateTime Time { get; set; }
    }
}
