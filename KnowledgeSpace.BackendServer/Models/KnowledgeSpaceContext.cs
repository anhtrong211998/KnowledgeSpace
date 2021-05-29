using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using KnowledgeSpace.BackendServer.Models.Entities;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Linq;
using KnowledgeSpace.BackendServer.Models.Interfaces;
using System;

namespace KnowledgeSpace.BackendServer.Models
{
    public class KnowledgeSpaceContext : IdentityDbContext<User>
    {
        public KnowledgeSpaceContext(DbContextOptions options) : base(options)
        {

        }

        /// <summary>
        /// OVERRIDES METHOD SAVECHANGES TO AUTO SET CREATE_DATE AND MODIFIED_DATE
        /// </summary>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns></returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<EntityEntry> modified = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
            foreach (EntityEntry item in modified)
            {
                if (item.Entity is IDateTracking changedOrAddedItem)
                {
                    if (item.State == EntityState.Added)
                    {
                        changedOrAddedItem.CreateDate = DateTime.Now;
                        changedOrAddedItem.LastModifiedDate = DateTime.Now;
                    }
                    else
                    {
                        changedOrAddedItem.LastModifiedDate = DateTime.Now;
                    }
                }
            }
            return base.SaveChangesAsync(cancellationToken);
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
