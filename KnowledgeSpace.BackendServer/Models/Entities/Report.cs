using KnowledgeSpace.BackendServer.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("Reports")]
    public class Report : IDateTracking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? KnowledgeBaseId { get; set; }

        public int? CommentId { get; set; }

        [MaxLength(500, ErrorMessage = "Report content is at most 500 characters.")]
        public string Content { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string ReportUserId { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public bool IsProcessed { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Type { get; set; }

        [ForeignKey("KnowledgeBaseId")]
        public virtual KnowledgeBase KnowledgeBase { get; set; }

        [ForeignKey("CommentId")]
        public virtual Comment Comment { get; set; }
    }
}
