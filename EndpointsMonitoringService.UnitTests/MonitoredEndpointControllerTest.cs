using System;
using EndpointsMonitoringService.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using EndpointsMonitoringService.Handlers;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using System.Net.Http;
using Moq.Protected;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using EndpointsMonitoringService.Controllers;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Collections.Generic;

namespace EndpointsMonitoringService.UnitTests
{
    public class MonitoredEndpointControllerTest
    {
        // GET: api/MonitoredEndpoint/5
        [Fact]
        public void GetMonitoredEndpoint_ForId_ValidUser_ReturnSameObject()
        {
            //ARANGE
            var fakeLoggerFactory = new NullLoggerFactory();
            var options = new DbContextOptionsBuilder<DatabaseContext>().
                UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var owner = new Owner();

            var fakeUsersList = FakeDataGenerator.GenerateUsers(2);

            owner.RegisterOwner(fakeUsersList[0]);
            var fakeEndpoint1 = FakeDataGenerator.GenerateEndpoint(1,owner.Data).Single();
            var fakeEndpoint2 = FakeDataGenerator.GenerateEndpoint(1,fakeUsersList[1],2).Single();


            using (var context = new DatabaseContext(options, fakeLoggerFactory))
            {
                context.User.AddRange(fakeUsersList);
                context.Add(fakeEndpoint1);
                context.Add(fakeEndpoint2);
                context.SaveChanges();

                var fakeHttpContext = new Mock<HttpContext>();
                var fakeHttpRequest = new Mock<HttpRequest>();
                fakeHttpContext.SetupGet<HttpRequest>(x => x.Request).Returns(fakeHttpRequest.Object);

                var handlerContext = new ControllerContext()
                {
                    HttpContext = fakeHttpContext.Object,
                };

                //ACT
                var monitoredEndpointController = new MonitoredEndpointController(owner, context, fakeLoggerFactory);
                var controllerResult = monitoredEndpointController.GetMonitoredEndpoint(1).Result;

                //ASSERT
                Assert.NotNull(controllerResult);
                Assert.NotNull(controllerResult.Value);
                Assert.IsType<MonitoredEndpoint>(controllerResult.Value);
                Assert.Equal(fakeEndpoint1, controllerResult.Value);
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public void GetMonitoredEndpoint_ForId_InvalidUser_ReturnEmpty()
        {
            //ARANGE
            var fakeLoggerFactory = new NullLoggerFactory();
            var options = new DbContextOptionsBuilder<DatabaseContext>().
                UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            var owner = new Owner();

            var fakeUsersList = FakeDataGenerator.GenerateUsers(2);

            owner.RegisterOwner(fakeUsersList[1]);

            var fakeMonitoredEndpointList = new List<MonitoredEndpoint>();
            fakeMonitoredEndpointList.AddRange(FakeDataGenerator.GenerateEndpoint(1, fakeUsersList[0]));
            fakeMonitoredEndpointList.AddRange(FakeDataGenerator.GenerateEndpoint(1, fakeUsersList[1],2));
            int idToGet = fakeMonitoredEndpointList.First(x => x.UserForeignKey != owner.Data.Id).Id;

            using (var context = new DatabaseContext(options, fakeLoggerFactory))
            {
                context.User.AddRange(fakeUsersList);
                context.MonitoredEndpoint.AddRange(fakeMonitoredEndpointList);
                context.SaveChanges();

                var fakeHttpContext = new Mock<HttpContext>();
                var fakeHttpRequest = new Mock<HttpRequest>();
                fakeHttpContext.SetupGet<HttpRequest>(x => x.Request).Returns(fakeHttpRequest.Object);

                var handlerContext = new ControllerContext()
                {
                    HttpContext = fakeHttpContext.Object,
                };

                //ACT
                var monitoredEndpointController = new MonitoredEndpointController(owner, context, fakeLoggerFactory);
                var controllerResult = monitoredEndpointController.GetMonitoredEndpoint(idToGet).Result;

                //ASSERT
                Assert.Null(controllerResult.Value); ;
                context.Database.EnsureDeleted();
            }
        }

        //todo...
    }
}
