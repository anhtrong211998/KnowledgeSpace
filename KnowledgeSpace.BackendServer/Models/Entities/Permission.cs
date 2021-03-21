using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgeSpace.BackendServer.Models.Entities
{
    [Table("Permissions")]
    public class Permission
    {
        public Permission(string functionId, string roleId, string commandId)
        {
            FunctionId = functionId;
            RoleId = roleId;
            CommandId = commandId;
        }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string FunctionId { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string RoleId { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string CommandId { get; set; }

        [ForeignKey("CommandId")]
        public virtual Command Command { get; set; }

        [ForeignKey("FunctionId")]
        public virtual Function Function { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
    }
}
