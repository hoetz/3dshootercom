using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Dnx.Runtime;
using System;
using Web.Domain;
using ElCamino.AspNet.Identity.AzureTable.Model;
using ElCamino.AspNet.Identity.AzureTable;
using Microsoft.AspNet.Identity;

namespace Web
{
    
    public class Startup
    {

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            // Setup configuration sources.
            var ConfigurationBuilder = new ConfigurationBuilder(appEnv.ApplicationBasePath)

#if DEBUG
          .AddJsonFile("secret.json")
#else
          .AddEnvironmentVariables();
#endif
          ;

            this.Configuration=ConfigurationBuilder.Build();

        }
        public IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
              services.AddScoped<IFeaturedArticlesQuery>((x)
                  => new FeaturedArticlesQuery(Configuration.GetSection("AzureConString").Value));
            services.AddScoped<IFrontPageService, FrontPageService>();
            
            // Add Identity services to the services container.
            services.AddIdentity<ApplicationUser, IdentityRole>((config) =>
            {
                
            })
                //.AddEntityFrameworkStores<ApplicationDbContext>()
                .AddAzureTableStores<IdentityCloudContext>(new Func<IdentityConfiguration>(() =>
                {
                    return new IdentityConfiguration() {
                        StorageConnectionString = Configuration.GetSection("AzureConString").Value
                    };
                }))
                .AddDefaultTokenProviders();
                
                services.AddScoped<SignInManager<ApplicationUser>, SignInManager<ApplicationUser>>();
                services.AddScoped<UserManager<ApplicationUser>, UserManager<ApplicationUser>>();
            
            services.AddMvc();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole();

            // Add the following to the request pipeline only in development environment.
            if (string.Equals(env.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
            {
                app.UseErrorPage();
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // send the request to the following path or controller action.
                app.UseErrorHandler("/Home/Error");
            }

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline.
            app.UseIdentity();

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });

                // Uncomment the following line to add a route for porting Web API 2 controllers.
                // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
            });

        }

    }
}
