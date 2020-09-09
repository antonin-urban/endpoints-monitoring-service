using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EndpointsMonitoringService.Model;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EndpointsMonitoringService.UnitTests
{
    public static class FakeDataGenerator
    {
        public static List<User> GenerateUsers(int count, int startId = 1)
        {
            var result = new List<User>();
            count += startId;

            for (int i = startId; i < count; i++)
            {
                User user = new User()
                {
                    Id = i,
                    UserName = string.Format("UserNumber{0}", i),
                    Email = string.Format("user.number{0}@test.com", i),
                    AccessToken = Guid.NewGuid()
                };

                result.Add(user);
            }

            return result;
        }

        public static List<MonitoredEndpoint> GenerateEndpoint(int count, User owner, int startId = 1)
        {
            var result = new List<MonitoredEndpoint>();
            count += startId;

            for (int i = startId; i < count; i++)
            {
                MonitoredEndpoint endpoint = new MonitoredEndpoint()
                {
                    Id = i,
                    Name = string.Format("EndopintNumber{0}", i),
                    Url = string.Format("www.number{0}.com", i),
                    UserForeignKey = owner.Id,
                    Owner = owner
                };

                result.Add(endpoint);
            }

            return result;
        }

        public static List<MonitoringResult> GenerateMonitoringResult(int count, MonitoredEndpoint endpoint, User owner, int startId = 1)
        {
            var result = new List<MonitoringResult>();
            count += startId;

            for (int i = startId; i < count; i++)
            {
                MonitoringResult monitoringResult = new MonitoringResult()
                {
                    Id = i,
                    DateOfCheck = DateTime.Now,
                    ReturnedHttpStatusCode = 100,
                    ReturnedPayload = String.Format("Payload for id {0}", i),
                    MonitoredEndpointForeignKey = endpoint.Id,
                    MonitoredEndpoint = endpoint
                };

                result.Add(monitoringResult);
            }

            return result;
        }

    }
}
