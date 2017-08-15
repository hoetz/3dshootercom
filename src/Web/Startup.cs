using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using Web.Domain;
using System.IO;
using Microsoft.AspNetCore.Identity;

namespace Web
{
    public class Startup
    {

        public Startup(IHostingEnvironment env)
        {

            // Setup configuration sources.
            var ConfigurationBuilder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath);

            if (env.IsDevelopment() && File.Exists(Path.Combine(env.ContentRootPath, "secret.json")))
            {
                ConfigurationBuilder
                    .AddJsonFile("secret.json");
            }
            else
            {
                ConfigurationBuilder
          .AddEnvironmentVariables();
            }

            this.Configuration = ConfigurationBuilder.Build();

        }
        public IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<TwitterSettings>(options => Configuration.GetSection("Twitter").Bind(options));
            services.Configure<DeploymentSlot>(options => Configuration.GetSection("DeploymentSlot").Bind(options));

            services.AddScoped<IFeaturedArticlesQuery>((x)
                  => new FeaturedArticlesQuery(Configuration.GetSection("AzureConString").Value));

            services.AddScoped<ITwitterQuery, TwitterQuery>();

            services.AddScoped<IAzureArticleQuery>((x)
            => new AzureArticleQuery(Configuration.GetSection("AzureConString").Value));
            services.AddScoped<IFrontPageService, FrontPageService>();
            services.AddScoped<IArticleService, ArticleService>();

            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration.GetSection("AzureSQLConString").Value));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/LogIn");
            services.AddAuthentication().AddTwitter(op =>
            {
                op.ConsumerKey = "ehWKBer0gENGJka9T5UMHHkED";
                op.ConsumerSecret = "l95fNu1ClgngGiLWy4qLmI7g0jiRiQ3hZAj5vl1W0wzL1SfPPy";
            });

            services.AddMvc();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole();

            // Add the following to the request pipeline only in development environment.
            if (string.Equals(env.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // send the request to the following path or controller action.
                app.UseExceptionHandler("/Home/Error");
            }

            // Add static files to the request pipeline.
            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline.
            app.UseAuthentication();



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
