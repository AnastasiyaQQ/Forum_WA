using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Forum.Entities
{
    [Table("Удаленная_запись")]
    public class DeletedPost
    {
        [Key] 
        [Column("id_записи")]
        public int PostId { get; set; } 

        [MaxLength(100)]
        [Column("Заголовок")]
        public string Title { get; set; }

        [Column("Дата_создания", TypeName = "DATE")]
        public DateTime? CreatedDate { get; set; }

        [Column("id_пользователя")]
        public int? UserId { get; set; } 

        [MaxLength(2000)]
        [Column("Содержание")]
        public string Content { get; set; }

        [Required]
        [Column("Дата_удаления", TypeName = "DATE")]
        public DateTime DeletedDate { get; set; }

    }
}
