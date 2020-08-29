using System;
using System.Security.Cryptography.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.Data.EntityFrameworkCore.Extensions;

namespace EndpointsMonitoringService.Model
{
    public class DatabaseContext : DbContext
    {
        private readonly ILogger<DatabaseContext> _logger;

        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<MonitoredEndpoint> MonitoredEndpoint { get; set; }
        public virtual DbSet<MonitoringResult> MonitoringResult { get; set; }


        public DatabaseContext(DbContextOptions<DatabaseContext> options, ILoggerFactory logger) : base(options)
        {
            _logger = logger.CreateLogger<DatabaseContext>();
            _logger.LogInformation("DatabaseContext constructor call");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserName).HasColumnName("user_name").IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").IsRequired();
                entity.Property(e => e.AccessToken).HasColumnName("access_token");
            });

            modelBuilder.Entity<MonitoredEndpoint>(entity =>
            {
                entity.ToTable("monitored_endpoint");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Url).HasColumnName("url").IsRequired();
                entity.Property(e => e.DateOfCreation).HasColumnName("date_of_creation").IsRequired();
                entity.Property(e => e.DateOfLastCheck).HasColumnName("date_of_last_check");
                entity.Property(e => e.MonitoredInterval).HasColumnName("monitored_interval"); //in s
                entity.Property(e => e.UserForeignKey).HasColumnName("fk_user").IsRequired();
                //entity.Property(e => e.Owner).IsRequired();
            });

            modelBuilder.Entity<MonitoringResult>(entity =>
            {
                entity.ToTable("monitoring_result");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.DateOfCheck).HasColumnName("date_of_check").IsRequired();
                entity.Property(e => e.ReturnedHttpStatusCode).HasColumnName("returned_http_status_code").IsRequired();
                entity.Property(e => e.ReturnedPayload).HasColumnName("returned_payload");
                entity.Property(e => e.MonitoredEndpointForeignKey).HasColumnName("fk_monitored_endpoint").IsRequired();


                //entity.Property(e => e.Owner).IsRequired();
            });

            modelBuilder.Entity<MonitoredEndpoint>()
           .HasOne(p => p.Owner)
           .WithMany(b => b.Endpoints)
           .HasForeignKey(p => p.UserForeignKey);

            modelBuilder.Entity<MonitoringResult>()
          .HasOne(p => p.MonitoredEndpoint)
          .WithMany(b => b.Results)
          .HasForeignKey(p => p.MonitoredEndpointForeignKey);




        }


    }
}
