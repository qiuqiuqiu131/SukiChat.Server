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
        [StringLength(50)]
        public string Password { get; set; }

        [StringLength(100)]
        public string? Introduction { get; set; }

        public int HeadIndex { get; set; }

        public int HeadCount { get; set; }

        [Required]
        public DateTime RegisteTime {  get; set; }
    }
}
