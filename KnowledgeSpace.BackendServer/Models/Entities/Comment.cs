using KnowledgeSpace.BackendServer.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("Comments")]
    public class Comment : IDateTracking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(500)]
        [Required]
        public string Content { get; set; }

        [Required]
        public int KnowledgeBaseId { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string OwnwerUserId { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        [ForeignKey("KnowledgeBaseId")]
        public virtual KnowledgeBase KnowledgeBase { get; set; }

        public virtual IList<Report> Reports { get; set; }

        public virtual IList<Attachment> Attachments { get; set; }
    }
}
