using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Forum.Entities
{
    [Table("Запись")]
    public class Post
    {
        [Key]
        [Column("id_записи")]
        public int Id { get; set; }

        [MaxLength(100)]
        [Column("Заголовок")]
        public string Title { get; set; }

        [Required]
        [Column("Дата_создания", TypeName = "DATE")] 
        public DateTime CreatedDate { get; set; }

        [Column("id_пользователя")]
        public int UserId { get; set; }

        [MaxLength(2000)]
        [Column("Содержание")]
        public string Content { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
