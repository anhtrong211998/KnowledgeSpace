using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("CommandInFunctions")]
    public class CommandInFunction
    {
        [Column(TypeName = "varchar(50)")]
        [MaxLength(50)]
        public string FunctionId { get; set; }

        [Column(TypeName = "varchar(50)")]
        [MaxLength(50)]
        public string CommandId { get; set; }

        [ForeignKey("CommandId")]
        public virtual Command Command { get; set; }

        [ForeignKey("FunctionId")]
        public virtual Function Function { get; set; }
    }
}
