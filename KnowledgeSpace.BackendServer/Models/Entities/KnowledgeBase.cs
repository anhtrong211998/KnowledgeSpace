﻿using KnowledgeSpace.BackendServer.Models.Interfaces;
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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? CategoryId { get; set; }

        [MaxLength(500, ErrorMessage = "Title is at most 500 characters.")]
        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "SeoAlias is at most 500 characters.")]
        [Required(ErrorMessage = "SeoAlias is required.")]
        [Column(TypeName = "varchar(500)")]
        public string SeoAlias { get; set; }

        [MaxLength(500, ErrorMessage = "Description is at most 500 characters.")]
        public string Description { get; set; }

        [MaxLength(500, ErrorMessage = "Environment is at most 500 characters.")]
        public string Environment { get; set; }

        [MaxLength(500, ErrorMessage = "Problem is at most 500 characters.")]
        public string Problem { get; set; }

        public string StepToReproduce { get; set; }

        [MaxLength(500, ErrorMessage = "ErrorMessage is at most 500 characters.")]
        public string ErrorMessage { get; set; }

        [MaxLength(500, ErrorMessage = "Workaround is at most 500 characters.")]
        public string Workaround { get; set; }

        public string Note { get; set; }

        [MaxLength(50, ErrorMessage = "OwnerUserId is at most 50 characters.")]
        [Required(ErrorMessage = "OwnerUserId is required.")]
        [Column(TypeName = "varchar(50)")]
        public string OwnerUserId { get; set; }

        public string Labels { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public int? NumberOfComments { get; set; }

        public int? NumberOfVotes { get; set; }

        public int? NumberOfReports { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public virtual IList<LabelInKnowledgeBase> LabelInKnowledgeBases { get; set; }

        public virtual IList<Comment> Comments { get; set; }

        public virtual IList<Report> Reports { get; set; }

        public virtual IList<Attachment> Attachments { get; set; }
    }
}