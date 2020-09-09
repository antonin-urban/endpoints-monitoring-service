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

            var fakeUsersList = FakeDataGenerator.GenerateUsers(2);

            owner.RegisterOwner(fakeUsersList[0]);

            var fakeMonitoredEndpointList = new List<MonitoredEndpoint>();
            fakeMonitoredEndpointList.AddRange(FakeDataGenerator.GenerateEndpoint(1, fakeUsersList[0]));
            fakeMonitoredEndpointList.AddRange(FakeDataGenerator.GenerateEndpoint(1, fakeUsersList[1], 2));

            var fakeMonitoringResultList = new List<MonitoringResult>();
            var expectedCollection = new List<MonitoringResult>();
            fakeMonitoringResultList.AddRange(FakeDataGenerator.GenerateMonitoringResult(11, fakeMonitoredEndpointList[0], fakeMonitoredEndpointList[0].Owner));
            expectedCollection = fakeMonitoringResultList.TakeLast(10).ToList();
            fakeMonitoringResultList.AddRange(FakeDataGenerator.GenerateMonitoringResult(5, fakeMonitoredEndpointList[1], fakeMonitoredEndpointList[1].Owner, 12));


            using (var context = new DatabaseContext(options, fakeLoggerFactory))
            {
                context.User.AddRange(fakeUsersList);
                context.MonitoredEndpoint.AddRange(fakeMonitoredEndpointList);
                context.MonitoringResult.AddRange(fakeMonitoringResultList);
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
                var controllerResult = monitoredEndpointController.GetMonitoringResultForEndopoint(fakeMonitoredEndpointList[0].Id).Result;

                //ASSERT
                Assert.NotNull(controllerResult);
                Assert.NotNull(controllerResult.Value);

                var resutlCollectionOrdered = controllerResult.Value.OrderBy(x => x.DateOfCheck).ToList();

                Assert.Equal(expectedCollection, resutlCollectionOrdered);
                context.Database.EnsureDeleted();
            }
        }

        //todo...
    }
}
