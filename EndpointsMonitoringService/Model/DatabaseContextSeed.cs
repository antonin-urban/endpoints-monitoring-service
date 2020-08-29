using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace EndpointsMonitoringService.Model
{
    public class DatabaseContextSeed
    {
        private readonly ILogger<DatabaseContextSeed> _logger;
        private readonly DatabaseContext _databaseContext;



        public DatabaseContextSeed(ILoggerFactory logger,DatabaseContext databaseContext)
        {
            _logger = logger.CreateLogger<DatabaseContextSeed>();
            _databaseContext = databaseContext;
            _logger.LogInformation("DatabaseContextSeed constructor call");
        }

        public void SeedUsers()
        {
            try
            {

                var user1 = new User()
                {
                    UserName = "Applifting",
                    Email = "info@applifting.cz",
                    AccessToken = "93f39e2f-80de-4033-99ee-249d92736a25"
                };


                if(!_databaseContext.User.Any(
                    x=>x.UserName == user1.UserName
                    && x.Email == user1.Email
                    && x.AccessToken == user1.AccessToken
                ))
                {
                    _databaseContext.Add(user1);
                    _databaseContext.SaveChangesAsync();
                }


                
                user1 = new User()
                {
                    UserName = "Batman",
                    Email = "b​atman@example.com",
                    AccessToken = "dcb20f8a-5657-4f1b-9f7f-ce65739b359e"
                };


                if (!_databaseContext.User.Any(
                   x => x.UserName == user1.UserName
                   && x.Email == user1.Email
                   && x.AccessToken == user1.AccessToken
               ))
                {
                    _databaseContext.Add(user1);
                    _databaseContext.SaveChangesAsync();
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR SEEDING USER");
            }
        }

        

    }
}
