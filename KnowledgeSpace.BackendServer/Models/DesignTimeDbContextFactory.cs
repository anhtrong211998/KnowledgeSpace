using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace KnowledgeSpace.BackendServer.Models
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<KnowledgeSpaceContext>
    {
        public KnowledgeSpaceContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<KnowledgeSpaceContext>();

            var connectionString = configuration.GetConnectionString("KnowledgeSpaceConnection");

            optionsBuilder.UseSqlServer(connectionString);

            return new KnowledgeSpaceContext(optionsBuilder.Options);
        }
    }
}