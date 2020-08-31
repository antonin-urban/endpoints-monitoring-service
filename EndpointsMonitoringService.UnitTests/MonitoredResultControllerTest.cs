using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EndpointsMonitoringService.Controllers;
using EndpointsMonitoringService.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MySql.Data.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace EndpointsMonitoringService.UnitTests
{
    public class MonitoredResultControllerTest
    {
        // GET: api/MonitoredEndpoint/5
        [Fact]
        public void GetMonitoredResult_ForEndpoint_ValidUser_Return10LastResults()
        {
            //ARANGE

            var fakeLoggerFactory = new NullLoggerFactory();
            var options = new DbContextOptionsBuilder<DatabaseContext>().
                UseInMemoryDatabase(DateTime.Now.ToString(Guid.NewGuid().ToString())).EnableDetailedErrors().Options;
            var owner = new Owner();

            var fakeUser1 = new Model.User()
            {
                Id = 1,
                UserName = "test",
                Email = "test@test.com",
                AccessToken = "1234-abcd",
            };

            var fakeUser2 = new Model.User()
            {
                Id = 2,
                UserName = "test2",
                Email = "test2@test.com",
                AccessToken = "5678-efgh",
            };

            owner.RegisterOwner(fakeUser1);
            var fakeEndpoint1 = new MonitoredEndpoint()
            {
                Id = 1,
                Name = "test",
                Url = "www.test.cz",
                DateOfCreation = DateTime.Now,
                UserForeignKey = 1,
                Owner = fakeUser1,
            };

            var fakeEndpoint2 = new MonitoredEndpoint()
            {
                Id = 2,
                Name = "test",
                Url = "www.test.cz",
                DateOfCreation = DateTime.Now,
                UserForeignKey =2,
                Owner = fakeUser2,
            };

            var expectedCollection = new List<MonitoringResult>();

            using (var context = new DatabaseContext(options, fakeLoggerFactory))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.Add(fakeUser1);
                context.Add(fakeUser2);
                context.Add(fakeEndpoint1);
                context.Add(fakeEndpoint2);

                for (int i = 1; i < 12; i++) //seed 11
                {
                    var monitoringResult = new MonitoringResult()
                    {
                        Id = i,
                        MonitoredEndpoint = fakeEndpoint1,
                        ReturnedHttpStatusCode = 0,
                        ReturnedPayload = string.Empty,
                    };
                    context.Add(monitoringResult);

                    if(i>1) //make sure it is LAST 10
                    {
                        expectedCollection.Add(monitoringResult);
                    }

                }

                for (int i = 94; i < 100; i++)
                {
                    var monitoringResult = new MonitoringResult()
                    {
                        Id = i,
                        MonitoredEndpoint = fakeEndpoint2,
                        ReturnedHttpStatusCode = 0,
                        ReturnedPayload = string.Empty,
                    };
                    context.Add(monitoringResult);


                }

                context.SaveChanges();
            
                var fakeHttpContext = new Mock<HttpContext>();
                var fakeHttpRequest = new Mock<HttpRequest>();
                fakeHttpContext.SetupGet<HttpRequest>(x => x.Request).Returns(fakeHttpRequest.Object);

                var handlerContext = new ControllerContext()
                {
                    HttpContext = fakeHttpContext.Object,
                };


                //ACT
                var monitoredEndpointController = new MonitoredResultController(owner, context, fakeLoggerFactory);
                var controllerResult = monitoredEndpointController.GetMonitoringResultForEndopoint(1).Result;

                //ASSERT
                Assert.NotNull(controllerResult);
                Assert.NotNull(controllerResult.Value);
                Assert.Equal(expectedCollection, controllerResult.Value);
                context.Database.EnsureDeleted();
            }
        }

        //todo...
    }
}
