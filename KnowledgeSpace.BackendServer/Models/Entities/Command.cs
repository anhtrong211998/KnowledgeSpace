using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("Commands")]
    public class Command
    {
        [Key]
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public virtual IList<CommandInFunction> commandInFunctions { get; set; }

        public virtual IList<Permission> Permissions { get; set; }
    }
}
