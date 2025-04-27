using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Forum.Entities
{
    [Table("Комментарий")]
    public class Comment
    {
        [Key]
        [Column("id_комментария")]
        public int Id { get; set; }

        [Column("id_записи")]
        public int PostId { get; set; }

        [Required]
        [Column("Дата_создания", TypeName = "DATE")]
        public DateTime CreatedDate { get; set; }

        [Column("id_пользователя")]
        public int UserId { get; set; }

        [MaxLength(2000)]
        [Column("Комментарий")] 
        public string Content { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
