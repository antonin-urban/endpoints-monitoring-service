using System;
using System.Security.Cryptography.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EndpointsMonitoringService.Model
{
    public class DatabaseContext : DbContext
    {
        private readonly ILogger<DatabaseContext> _logger;


        public DatabaseContext(DbContextOptions<DatabaseContext> options, ILogger<DatabaseContext> logger) : base(options)
        {
            _logger = logger;
            logger.LogInformation("DatabaseContext constructor");
        }
    }
}
