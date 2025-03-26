using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ChatServer.DataBase.DataBase.DataEntity
{
    public class FriendRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        [ForeignKey(nameof(UserFrom))]
        public string UserFromId {  get; set; }
        [Required]
        public User UserFrom { get; set; }

        [Required]
        [StringLength(10)]
        [ForeignKey(nameof(UserTarget))]
        public string UserTargetId { get; set; }
        [Required]
        public User UserTarget { get; set; }

        [Required]
        public string Group { get; set; }

        [Required]
        public string Remark { get; set; }

        [Required]
        public DateTime RequestTime { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public bool IsAccept { get; set; } = false;

        [Required]
        public bool IsSolved { get; set; } = false;

        public DateTime? SolveTime { get; set; }
    }
}
