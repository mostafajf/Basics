using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Basics.Data;
using Microsoft.AspNetCore.Identity;
using Basics.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Debug;
namespace Basics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IWebHost host = BuildWebHost(args);
            InitData(host);
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
                .UseStartup<Startup>()
                .ConfigureLogging((hostContext, logger) =>
                {
                    //CreateDefaultBuilder enable this by default
                    logger.AddConsole();
                    logger.AddDebug();
                    logger.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    //add for all providers
                    logger.AddFilter("System", LogLevel.Error);
                    //add for Debug Provider
                    logger.AddFilter<DebugLoggerProvider>("Microsoft", LogLevel.Critical);
                })
                .Build();
        public static void InitData(IWebHost host)
        {
            var serviceFactory = host.Services.GetService<IServiceScopeFactory>();
            using (var scope = serviceFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                ApplicationUser mostafa = db.Users.FirstOrDefault(u => u.UserName == "mustafa.gholipoor@gmail.com");
                if (db.UserClaims.Count(c => c.UserId == mostafa.Id) > 0)
                {
                    return;
                }
                IdentityUserClaim<string> DateOfBirth = new IdentityUserClaim<string>();
                DateOfBirth.ClaimType = ClaimTypes.DateOfBirth;
                DateOfBirth.ClaimValue = "1985-1-1";
                DateOfBirth.UserId = mostafa.Id;
                db.UserClaims.Add(DateOfBirth);
                db.SaveChanges();

            }

        }
    }
}
