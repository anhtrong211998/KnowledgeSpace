using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("Functions")]
    public class Function
    {
        [Key]
        [MaxLength(50, ErrorMessage = "FunctionID is at most 50 characters.")]
        [Column(TypeName = "varchar(50)")]
        public string Id { get; set; }

        [MaxLength(200, ErrorMessage = "Function name is at most 200 characters.")]
        [Required(ErrorMessage = "Function name is required.")]
        public string Name { get; set; }

        [MaxLength(200, ErrorMessage = "Function Url is at most 200 characters.")]
        [Required(ErrorMessage = "Function Url is required.")]
        public string Url { get; set; }

        [Required(ErrorMessage = "Function SortOrder is required.")]
        public int SortOrder { get; set; }

        [MaxLength(50, ErrorMessage = "ParentId is at most 50 characters.")]
        [Column(TypeName = "varchar(50)")]
        public string ParentId { get; set; }

        public virtual IList<CommandInFunction> CommandInFunctions { get; set; }

        public virtual IList<Permission> Permissions { get; set; }
    }
}
