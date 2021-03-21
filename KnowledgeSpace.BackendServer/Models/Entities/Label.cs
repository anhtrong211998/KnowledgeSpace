using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("Labels")]
    public class Label
    {
        [Key]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        public virtual IList<LabelInKnowledgeBase> LabelInKnowledgeBases { get; set; }
    }
}
