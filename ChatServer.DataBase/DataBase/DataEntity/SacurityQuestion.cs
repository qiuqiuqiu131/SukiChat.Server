using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.DataEntity
{
    public class SacurityQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Question { get; set; }

        [Required]
        [StringLength(50)]
        public string Answer { get; set; }

        [Required]
        [StringLength(10)]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }

        [Required]
        public User User { get; set; }
    }
}
