using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using KnowledgeSpace.BackendServer.Models.Entities;

namespace KnowledgeSpace.BackendServer.Models
{
    public class KnowledgeSpaceContext : IdentityDbContext<User>
    {
        public KnowledgeSpaceContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<ActivityLog> ActivityLogs { get; set; }

        public DbSet<Attachment> Attachments { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Command> Commands { get; set; }

        public DbSet<CommandInFunction> CommandInFunctions { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Function> Functions { get; set; }

        public DbSet<KnowledgeBase> KnowledgeBases { get; set; }

        public DbSet<Label> Labels { get; set; }

        public DbSet<LabelInKnowledgeBase> LabelInKnowledgeBases { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        public DbSet<Report> Reports { get; set; }

        public DbSet<Vote> Votes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>()
                .ToTable("Roles")
                .Property(x => x.Id)
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Entity<User>()
                .ToTable("Users")
                .Property(x => x.Id)
                .HasMaxLength(50)
                .IsUnicode(false);

            builder.Entity<IdentityUserRole<string>>()
                .ToTable("UserRoles");

            builder.Entity<CommandInFunction>()
                .HasKey(x => new { x.CommandId, x.FunctionId });

            builder.Entity<Permission>()
                .HasKey(p => new { p.CommandId, p.FunctionId, p.RoleId });

            builder.Entity<LabelInKnowledgeBase>()
                .HasKey(l => new { l.LabelId, l.KnowledgeBaseId });

            builder.Entity<Vote>()
                .HasKey(v => new { v.KnowledgeBaseId, v.UserId });

            builder.HasSequence("KnowledgeBaseSequence");

        }
    }
}
