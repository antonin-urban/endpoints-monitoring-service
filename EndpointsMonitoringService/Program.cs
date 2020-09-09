using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EndpointsMonitoringService.Model;
using EndpointsMonitoringService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EndpointsMonitoringService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();
                Log.Logger.ForContext(typeof(Program)).Information("APP STARTED");
                host.Run();
            }
            catch (Exception ex)
            {
                Log.ForContext(typeof(Program)).Error(ex, "APP TERMINATED UNEXPECTEDLY");

            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
            .ConfigureServices(services =>
                {
            services.AddHostedService<MonitoringWorker>();
            services.AddHttpClient();
        });
    }
}
