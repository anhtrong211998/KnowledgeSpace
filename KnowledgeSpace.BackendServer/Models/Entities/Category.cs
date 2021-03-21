using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(200, ErrorMessage = "Category name is at most 200 character")]
        [Required(ErrorMessage = "Category name is required.")]
        public string Name { get; set; }

        [MaxLength(200, ErrorMessage = "SeoAlias is at most 200 character")]
        [Column(TypeName = "varchar(200)")]
        [Required(ErrorMessage = "SeoAlias is required.")]
        public string SeoAlias { get; set; }

        [MaxLength(500, ErrorMessage = "SeoDescription is at most 200 character")]
        public string SeoDescription { get; set; }

        [Required(ErrorMessage = "SortOrder is required.")]
        public int SortOrder { get; set; }

        public int? ParentId { get; set; }

        public int? NumberOfTickets { get; set; }

        public virtual IList<KnowledgeBase> KnowledgeBases { get; set; }
    }
}
