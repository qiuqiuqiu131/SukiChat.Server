using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.DataEntity
{
    public class GroupRelation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Group))]
        [StringLength(10)]
        public string GroupId { get; set; }
        public Group Group { get; set; }

        [ForeignKey(nameof(User))]
        [StringLength(10)]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int Status { get; set; }

        [Required]
        [StringLength(20)]
        public string Grouping { get; set; }

        [Required]
        public DateTime JoinTime { get; set; }

        [StringLength(30)]
        public string? NickName { get; set; }

        [StringLength(30)]
        public string? Remark { get; set; }

        [Required]
        public bool CantDisturb { get; set; } = false;

        [Required]
        public bool IsTop { get; set; } = false;
    }
}
