using EventAuthServer.Entity;
using EventAuthServer.ModelBuilderExtension;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Elasticsearch;
using System;
using System.IO;
using System.Reflection;

namespace EventAuthServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{environment}.json",
                    optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Create the Serilog logger, and configure the sinks
            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().Enrich.WithExceptionDetails().WriteTo.Console(new RenderedCompactJsonFormatter())
            .WriteTo.Elasticsearch(ConfigureElasticSink(config, environment))
            .Enrich.WithProperty("Environment", environment)
            .ReadFrom.Configuration(config)
            .CreateLogger();

            Log.Information("Starting host");
            // Wrap creating and running the host in a try-catch block
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    var configDb = config.GetSection("DbConfig");
                    var migration = configDb.GetValue<bool>("Migrate");

                    if (context.Database.IsSqlServer() && migration)
                    {
                        context.Database.Migrate();
                    }
                    var seed = configDb.GetValue<bool>("Seed");

                    if (seed)
                    {
                        var userManager = services.GetRequiredService<UserManager<AppUserModel>>();
                        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<string>>>();

                        ModelBuilderExtensionsHelper.SeedData(context, userManager, roleManager, config).Wait();
                    }
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                    Log.Fatal(ex, "Host terminated unexpectedly");
                    throw;
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
              .UseSerilog()
              .ConfigureWebHostDefaults(webBuilder =>
              {
                  webBuilder.UseStartup<Startup>();
                  webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                  webBuilder.UseUrls("https://*:44321");
                  webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                  {
                      var env = hostingContext.HostingEnvironment;
                      config.SetBasePath(env.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile(path: $"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                      config.AddEnvironmentVariables();
                  });
                  webBuilder.CaptureStartupErrors(true);
                  webBuilder.UseIISIntegration();
              });
        private static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
        {
            return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
            };
        }
    }
}
