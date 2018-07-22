using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Basics.Data;
using Basics.Models;
using Basics.Services;
using System.Diagnostics;
using Basics.MiddleWares;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Basics.Bussiness;
using Microsoft.AspNetCore.Authorization;

namespace Basics
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Enviroment = environment;
            //Configuration = configuration;
            var builder = new ConfigurationBuilder();
            if (environment.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }
            builder.SetBasePath(environment.ContentRootPath);
            builder.AddJsonFile("appsettings.json", false, true);
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Enviroment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
            //user seceret
            var secret = Configuration["MySecret"];

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
                //sign in
                //options.SignIn.RequireConfirmedEmail = true;

            });
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                // If the LoginPath isn't set, ASP.NET Core defaults 
                // the path to /Account/Login.
                options.LoginPath = "/Account/Login";
                // If the AccessDeniedPath isn't set, ASP.NET Core defaults 
                // the path to /Account/AccessDenied.
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = false;
            });

            services.AddDirectoryBrowser();
            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IAuthorizationHandler, MinimumAgeHandler>();
            services.AddScoped<IAuthorizationHandler, HasBadgeHandler>();
            services.AddScoped<IAuthorizationHandler, HasTemporaryPassHandler>();
            services.AddScoped<IAuthorizationHandler, StudentOwnerHandler>();

            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = "700529558319-5v7bpevqrj3mtd93kn57mijc7c8lem95.apps.googleusercontent.com";
                //for production use secret insteed of secret manager
                options.ClientSecret = Configuration["GoogleSecret"];
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Administrators"));
                options.AddPolicy("EmployeeOnly", policy => policy.RequireClaim("EmployeeOnly"));
                options.AddPolicy("Founders", policy => policy.RequireClaim("EmployeeNumber", "1", "2", "3", "4", "5"));
                options.AddPolicy("Over21Only", policy => policy.AddRequirements(new MinimumAgeRequirement(21)));
                options.AddPolicy("BuildingEntry", policy => policy.Requirements.Add(new OfficeEntryRequirement()));
            });
            services.AddMvc();
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime life)
        {
            life.ApplicationStarted.Register(OnStarted);
            life.ApplicationStopping.Register(OnStopping);
            life.ApplicationStopped.Register(OnStopped);
            //life.StopApplication();
            StaticFiles(app);
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                //The Windows EventLog provider
                //loggerFactory.AddEventSourceLogger();
                // add Trace Source logging
                var testSwitch = new SourceSwitch("sourceSwitch", "Logging Sample");
                testSwitch.Level = SourceLevels.Warning;
                loggerFactory.AddTraceSource(testSwitch,
                    new TextWriterTraceListener(writer: Console.Out));
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            //app.Run delegate terminates the pipeline
            //app.Run(async context =>
            //{
            //    await context.Response.WriteAsync("hello word . short cirquit!");
            //}
            //);
            app.Use(async (context, next) =>
            {
                // Do work that doesn't write to the Response.
                await next.Invoke();
                // Do logging or other work that doesn't write to the Response.
            });
            app.Map("/map1", appctx => appctx.Run(async ctx => await ctx.Response.WriteAsync("request filter")));
            app.UseRequestCulture();
            app.UseAuthentication();

            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "ENNumber",
                     template: "en-Us/Number/{id}",
                     defaults: new { controller = "Home", action = "Number" },
                     constraints: new { id = new IntRouteConstraint() },
                     dataTokens: new { locale = "en-US" }
                     );
            });
            CustomeRouteHandler(app);
            //It's often a good idea for production error pages to consist of purely static content
            app.UseStatusCodePages();
            //Sende Status Code to Route
            //app.UseStatusCodePagesWithRedirects("/error/{0}");
            //app.UseStatusCodePagesWithReExecute("/error/{0}");
        }
        public void StaticFiles(IApplicationBuilder app)
        {
            // Serve my app-specific default file, if present.
            DefaultFilesOptions options = new DefaultFilesOptions();
            options.DefaultFileNames.Clear();
            options.DefaultFileNames.Add("mydefault.html");
            app.UseDefaultFiles(options);
            app.UseStaticFiles(new StaticFileOptions
            {
                //Enabling ServeUnknownFileTypes is a security risk. It's disabled by default, and its use is discouraged.
                //FileExtensionContentTypeProvider provides a safer alternative to serving files with non-standard extensions.
                ServeUnknownFileTypes = true,
                DefaultContentType = "images/png"
            });// For the wwwroot folder
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles")),
                RequestPath = "/MyFiles",
                OnPrepareResponse = ctx =>
                {
                    //The files have been made publicly cacheable for 10 minutes (600 seconds):
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
                    System.Net.Http.Headers.CacheControlHeaderValue cc = new System.Net.Http.Headers.CacheControlHeaderValue
                    {
                        MaxAge = TimeSpan.FromSeconds(600),
                        Public = true
                    };
                }
            });
            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".myapp"] = "application/x-msdownload";
            provider.Mappings[".htm3"] = "text/html";
            provider.Mappings[".image"] = "image/svg+xml";
            // Replace an existing mapping
            provider.Mappings[".rtf"] = "application/x-msdownload";
            // Remove MP4 videos.
            provider.Mappings.Remove(".mp4");
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
           Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images")),
                RequestPath = "/MyImages",
                ContentTypeProvider = provider
            });
            //Disabling directory browsing in production is highly recommended
            //app.UseDirectoryBrowser(new DirectoryBrowserOptions
            //{
            //    FileProvider = new PhysicalFileProvider(
            //            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images")),
            //    RequestPath = "/MyImages",

            //});

        }
        public void CustomeRouteHandler(IApplicationBuilder app)
        {
            var packageRouteHandler = new RouteHandler(context =>
            {

                var routevalues = context.GetRouteData().Values;
                return context.Response.WriteAsync($"Hello RouteValues {string.Join(',', routevalues)}");
            });
            var routeBuilder = new RouteBuilder(app, packageRouteHandler);
            routeBuilder.MapRoute(
            "Track Package Route",
            "package/{operation:regex(^(track|create|detonate)$)}/{id:int}");
            routeBuilder.MapGet("hello/{name}", context =>
            {
                var name = context.GetRouteValue("name");
                // This is the route handler when HTTP GET "hello/<anything>"  matches
                // To match HTTP GET "hello/<anything>/<anything>,
                // use routeBuilder.MapGet("hello/{*name}"
                return context.Response.WriteAsync($"Hi, {name}!");
            });
            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }
        private void OnStarted()
        {
            // Perform post-startup activities here
        }

        private void OnStopping()
        {
            // Perform on-stopping activities here
        }

        private void OnStopped()
        {
            // Perform post-stopped activities here
        }
    }
}
