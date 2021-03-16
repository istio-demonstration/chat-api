using System;
using System.Reflection;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
               var context = services.GetRequiredService<DataContext>();
               var userManager = services.GetRequiredService<UserManager<AppUser>>();
               var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
               await context.Database.MigrateAsync();
               await Seed.SeedUsers(userManager, roleManager);
            }
            catch (Exception e)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(e, "An error occurred during migration");
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
        {
            var env = hostingContext.HostingEnvironment;
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
            config.AddEnvironmentVariables();
        })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var assemblyName = typeof(Startup).GetTypeInfo().Assembly.FullName;
                    webBuilder.UseStartup(assemblyName ?? string.Empty);
                    // webBuilder.UseStartup<Startup>();

                    webBuilder.UseUrls("http://+:5000");
                    webBuilder.UseUrls("https://+:5001");
                });
    }
}
