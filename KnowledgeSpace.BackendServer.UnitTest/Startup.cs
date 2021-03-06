using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.BackendServer.Models.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using FluentValidation.AspNetCore;
using KnowledgeSpace.ViewModels.Validators;

namespace KnowledgeSpace.BackendServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //// 1. Setup entity framework
            services.AddDbContext<KnowledgeSpaceContext>(options =>
                options.UseSqlServer(
                        Configuration.GetConnectionString("KnowledgeSpaceConnection")
                        )
                );

            //// 2. Setup identity
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<KnowledgeSpaceContext>();

            services.Configure<IdentityOptions>(options =>
            {
                //// Default Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.User.RequireUniqueEmail = true;
            });

            //// validator use fluent validator library
            services.AddControllers()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<RoleCreateRequestValidator>());

            //// 3. Add trasient to seed data
            services.AddTransient<DbInitializer>();

            //// Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Knowledge Space API",
                    Version = "v1"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //// Swagger and Endpont for swagger
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Knowledge Space API v1");
            });
        }
    }
}
