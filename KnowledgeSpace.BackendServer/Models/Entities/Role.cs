using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("Roles")]
    public class Role : IdentityRole
    {
        public Role() : base()
        {
        }

        public virtual IList<Permission> Permissions { get; set; }
    }
}
