using System.ComponentModel.DataAnnotations;

namespace ChatServer.DataBase.DataBase.DataEntity
{
    public class User
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Name { get; set; }

        [Required]
        public bool IsMale { get; set; } = true;

        public DateOnly? Birth { get; set; }

        [Required]
        [StringLength(18)]
        public string Password { get; set; }

        [StringLength(100)]
        public string? Introduction { get; set; }

        public int HeadIndex { get; set; }

        public int HeadCount { get; set; }

        public DateTime LastReadFriendMessageTime { get; set; } = DateTime.MinValue;

        public DateTime LastReadGroupMessageTime { get; set; } = DateTime.MinValue;

        public DateTime LastDeleteFriendMessageTime { get; set; } = DateTime.MinValue;

        public DateTime LastDeleteGroupMessageTime { get; set; } = DateTime.MinValue;

        [Required]
        public DateTime RegisteTime {  get; set; }
    }
}
