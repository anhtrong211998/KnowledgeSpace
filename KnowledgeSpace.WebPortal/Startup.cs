using FluentValidation.AspNetCore;
using IdentityModel.Client;
using KnowledgeSpace.ViewModels.Validators;
using KnowledgeSpace.WebPortal.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KnowledgeSpace.WebPortal
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
            services.AddHttpClient();

            //IdentityModelEventSource.ShowPII = true; //Add this line
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = Configuration["Authorization:AuthorityUrl"];
                    options.RequireHttpsMetadata = false;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.ClientId = Configuration["Authorization:ClientId"];
                    options.ClientSecret = Configuration["Authorization:ClientSecret"];
                    options.ResponseType = "code";

                    options.SaveTokens = true;

                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("offline_access");
                    options.Scope.Add("api.knowledgespace");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });

            var builder = services.AddControllersWithViews()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<KnowledgeBaseCreateRequestValidator>());
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == Environments.Development)
            {
                builder.AddRazorRuntimeCompilation();
            }

            //Declare DI containers
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<ICategoryApiClient, CategoryApiClient>();
            services.AddTransient<IKnowledgeBaseApiClient, KnowledgeBaseApiClient>();
            services.AddTransient<ILabelApiClient, LabelApiClient>();
            services.AddTransient<IUserApiClient, UserApiClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                RoutingBuilder(endpoints);
            });
        }
        private static void RoutingBuilder(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapControllerRoute(
                    name: "Edit KB",
                    pattern: "/edit-kb/{id}",
                    new { controller = "Account", action = "EditKnowledgeBase" });

                endpoints.MapControllerRoute(
                    name: "My KBs",
                    pattern: "/my-kbs",
                    new { controller = "Account", action = "MyKnowledgeBases" });
                endpoints.MapControllerRoute(
                    name: "New KB",
                    pattern: "/new-kb",
                    new { controller = "Account", action = "CreateNewKnowledgeBase" });

                endpoints.MapControllerRoute(
                    name: "List By Tag Id",
                    pattern: "/tag/{tagId}",
                    new { controller = "KnowledgeBase", action = "ListByTag" });

                endpoints.MapControllerRoute(
                    name: "Search KB",
                    pattern: "/search",
                    new { controller = "KnowledgeBase", action = "Search" });

                endpoints.MapControllerRoute(
                    name: "KnowledgeBaseDetails",
                    pattern: "/knowledgebase/{seoAlias}-{id}",
                    new { controller = "KnowledgeBase", action = "Details" });

                endpoints.MapControllerRoute(
                    name: "ListByCategoryId",
                    pattern: "/category/{categoryAlias}-{id}",
                    new { controller = "KnowledgeBase", action = "ListByCategoryId" });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}
