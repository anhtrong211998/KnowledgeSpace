using KnowledgeSpace.BackendServer.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("ActivityLogs")]
    public class ActivityLog : IDateTracking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(50, ErrorMessage = "Action is at most 50 characters.")]
        [Required(ErrorMessage = "Action is required.")]
        public string Action { get; set; }

        [MaxLength(50, ErrorMessage = "EntityName is at most 50 characters.")]
        [Required(ErrorMessage = "EntityName is required.")]
        public string EntityName { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        [Required]
        public string EntityId { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string UserId { get; set; }

        [MaxLength(500, ErrorMessage = "Content is at most 500 characters.")]
        public string Content { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
