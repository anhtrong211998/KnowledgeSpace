using KnowledgeSpace.BackendServer.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("KnowledgeBases")]
    public class KnowledgeBase : IDateTracking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? CategoryId { get; set; }

        [MaxLength(500)]
        [Required]
        public string Title { get; set; }

        [MaxLength(500)]
        [Required]
        [Column(TypeName = "varchar(500)")]
        public string SeoAlias { get; set; }

        public string Description { get; set; }

        public string Environment { get; set; }

        public string Problem { get; set; }

        public string StepToReproduce { get; set; }

        public string ErrorMessage { get; set; }

        public string Workaround { get; set; }

        public string Note { get; set; }

        [MaxLength(50)]
        [Required]
        [Column(TypeName = "varchar(50)")]
        public string OwnerUserId { get; set; }

        public string Labels { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public int? NumberOfComments { get; set; }

        public int? NumberOfVotes { get; set; }

        public int? NumberOfReports { get; set; }

        public bool Approved { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public virtual IList<LabelInKnowledgeBase> LabelInKnowledgeBases { get; set; }

        public virtual IList<Comment> Comments { get; set; }

        public virtual IList<Report> Reports { get; set; }

        public virtual IList<Attachment> Attachments { get; set; }
    }
}
