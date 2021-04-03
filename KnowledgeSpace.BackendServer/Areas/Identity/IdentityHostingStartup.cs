using System;
using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.BackendServer.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(KnowledgeSpace.BackendServer.Areas.Identity.IdentityHostingStartup))]
namespace KnowledgeSpace.BackendServer.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}