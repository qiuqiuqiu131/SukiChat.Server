using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.DataEntity
{
    /// <summary>
    /// 记录客户端发送的加入群聊申请
    /// </summary>
    public class GroupRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        [ForeignKey(nameof(UserFrom))]
        public string UserFromId { get; set; }
        [Required]
        public User UserFrom { get; set; }

        [Required]
        [StringLength(10)]
        [ForeignKey(nameof(Group))]
        public string GroupId { get; set; }
        [Required]
        public Group Group { get; set; }

        [Required]
        public DateTime RequestTime { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public bool IsAccept { get; set; } = false;

        [Required]
        public bool IsSolved { get; set; } = false;

        public DateTime? SolveTime { get; set; }

        [StringLength(10)]
        public string? AcceptByUserId { get; set; }
    }
}
