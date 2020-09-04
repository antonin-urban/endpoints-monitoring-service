using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EndpointsMonitoringService.Handlers;
using EndpointsMonitoringService.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EndpointsMonitoringService
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
            services.AddDbContext<DatabaseContext>(options =>
            options.UseMySQL(Configuration.GetConnectionString("EndpointsMonitoringServiceDatabase")));
            services.AddControllers();

            services.AddAuthentication("Basic Bearer Token Auhtentication")
            .AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>("Basic Bearer Token Auhtentication", null);

            services.AddScoped<IOwner, Owner>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory logger)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .Build();

                Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            try
            {
                using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                {
                    var dbContext = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();

                    if (env.IsDevelopment())
                    {
                        //dbContext.Database.EnsureDeleted();
                        //Log.Logger.ForContext(typeof(Program)).Information("Database EnsureDeleted done");
                    }
                    dbContext.Database.EnsureCreated();
                    Log.Logger.ForContext(typeof(Program)).Information("Database EnsureCreated done");

                    DatabaseContextSeed dbSeed = new DatabaseContextSeed(logger, dbContext);
                    dbSeed.SeedUsers();

                }
            }
            catch (Exception ex)
            {
                Log.Logger.ForContext(typeof(Program)).Error(ex, "ERROR ENSURE CREATED DATABASE");
                throw ex;
            }
        }
    }
}
