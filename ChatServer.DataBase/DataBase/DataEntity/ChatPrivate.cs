using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.DataEntity
{
    public class ChatPrivate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(UserFrom))]
        [StringLength(10)]
        public string UserFromId {  get; set; }
        [Required]
        public User UserFrom { get; set; }

        [Required]
        [ForeignKey(nameof(UserTarget))]
        [StringLength(10)]
        public string UserTargetId { get; set; }
        [Required]
        public User UserTarget { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime Time { get; set; }
    }
}
