using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("LabelInKnowledgeBases")]
    public class LabelInKnowledgeBase
    {
        public int KnowledgeBaseId { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string LabelId { get; set; }

        [ForeignKey("KnowledgeBaseId")]
        public virtual KnowledgeBase KnowledgeBase { get; set; }

        [ForeignKey("LabelId")]
        public virtual Label Label { get; set; }
    }
}
