using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("Commands")]
    public class Command
    {
        [Key]
        [MaxLength(50, ErrorMessage = "CommandID is at most 50 characters.")]
        [Column(TypeName = "varchar(50)")]
        public string Id { get; set; }

        [Required(ErrorMessage = "Command name is required.")]
        [MaxLength(50, ErrorMessage = "Command name is at most 50 characters.")]
        public string Name { get; set; }

        public virtual IList<CommandInFunction> commandInFunctions { get; set; }

        public virtual IList<Permission> Permissions { get; set; }
    }
}
