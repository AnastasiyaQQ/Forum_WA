using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Forum.Entities
{
    [Table("Роль")]
    public class Role
    {
        [Key]
        [Column("id_роли")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Роль")]
        public string Name { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
