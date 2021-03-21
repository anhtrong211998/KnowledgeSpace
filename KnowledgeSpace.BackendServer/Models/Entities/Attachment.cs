using KnowledgeSpace.BackendServer.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("Attachments")]
    public class Attachment : IDateTracking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "FileName is required.")]
        [MaxLength(200, ErrorMessage = "FileName is at most 200 characters.")]
        public string FileName { get; set; }

        [Required(ErrorMessage = "FilePath is required.")]
        [MaxLength(200, ErrorMessage = "FilePath is at most 200 characters.")]
        public string FilePath { get; set; }

        [Required(ErrorMessage = "FileType is required.")]
        [MaxLength(4, ErrorMessage = "FileType is at most 4 characters.")]
        [Column(TypeName = "varchar(4)")]
        public string FileType { get; set; }

        [Required(ErrorMessage = "FileSize is required.")]
        public long FileSize { get; set; }

        public int? KnowledgeBaseId { get; set; }

        public int? CommentId { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        [MaxLength(10, ErrorMessage = "Type is at most 10 characters.")]
        [Column(TypeName = "varchar(10)")]
        public string Type { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        [ForeignKey("CommentId")]
        public virtual Comment Comment { get; set; }

        [ForeignKey("KnowledgeBaseId")]
        public virtual KnowledgeBase KnowledgeBase { get; set; }
    }
}
