using KnowledgeSpace.BackendServer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KnowledgeSpace.BackendServer.UnitTest
{
    public class InMemoryDbContextFactory
    {
        public KnowledgeSpaceContext GetApplicationDbContext()
        {
            var options = new DbContextOptionsBuilder<KnowledgeSpaceContext>()
                       .UseInMemoryDatabase(databaseName: "InMemoryApplicationDatabase")
                       .Options;
            var dbContext = new KnowledgeSpaceContext(options);

            return dbContext;
        }
    }
}
