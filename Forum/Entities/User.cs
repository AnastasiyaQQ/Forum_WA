using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Forum.Entities
{
    [Table("Пользователь")]
    public class User
    {
        [Key]
        [Column("id_пользователя")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("Имя")]
        public string Username { get; set; } 

        [Required]
        [MaxLength(250)] 
        [Column("Пароль")]
        public string PasswordHash { get; set; }

        [Column("id_роли")]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
