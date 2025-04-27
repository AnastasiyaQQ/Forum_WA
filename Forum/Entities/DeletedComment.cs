using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Forum.Entities
{
    [Table("Удаленный_комментарий")]
    public class DeletedComment
    {
        [Key]
        [Column("id_комментария")]
        public int CommentId { get; set; } 

        [Column("id_записи")]
        public int? PostId { get; set; }

        [Column("Дата_создания", TypeName = "DATE")]
        public DateTime? CreatedDate { get; set; }

        [Column("id_пользователя")]
        public int? UserId { get; set; }

        [MaxLength(2000)]
        [Column("Комментарий")]
        public string Content { get; set; }

        [Required]
        [Column("Дата_удаления", TypeName = "DATE")]
        public DateTime DeletedDate { get; set; }
    }
}
