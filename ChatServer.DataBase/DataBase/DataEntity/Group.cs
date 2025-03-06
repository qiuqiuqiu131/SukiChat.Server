using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.DataBase.DataBase.DataEntity
{
    public class Group
    {
        [Key]
        [StringLength(10)]
        public string Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        [Required]
        public DateTime CreateTime { get; set; }

        [Required]
        public string? HeadPath { get; set; }
    }
}
