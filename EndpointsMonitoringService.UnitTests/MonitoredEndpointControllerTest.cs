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
                UseInMemoryDatabase(DateTime.Now.ToString("HHmmssffffzzz")).Options;
            var owner = new Owner();

            var fakeUser = new Model.User()
            {
                Id = 1,
                UserName = "test",
                Email = "test@test.com",
                AccessToken = Guid.NewGuid(),
            };

            owner.RegisterOwner(fakeUser);
            var fakeEndpoint = new MonitoredEndpoint()
            {
                Id = 1,
                Name = "test",
                Url = "www.test.cz",
                DateOfCreation = DateTime.Now,
                UserForeignKey = 1,
                Owner = fakeUser,
            };

            using (var context = new DatabaseContext(options, fakeLoggerFactory))
            {
                context.Add(fakeUser);
                context.Add(fakeEndpoint);
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
                Assert.Equal(fakeEndpoint, controllerResult.Value);
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

            var fakeUser1 = new Model.User()
            {
                Id = 1,
                UserName = "test",
                Email = "test@test.com",
                AccessToken = Guid.NewGuid(),
            };

            var fakeUser2 = new Model.User()
            {
                Id = 2,
                UserName = "test2",
                Email = "test2@test.com",
                AccessToken = Guid.NewGuid(),
            };

            owner.RegisterOwner(fakeUser2);

            var fakeEndpoint = new MonitoredEndpoint()
            {
                Id = 1,
                Name = "test",
                Url = "www.test.cz",
                DateOfCreation = DateTime.Now,
                UserForeignKey = 1,
                Owner = fakeUser1,
            };

            using (var context = new DatabaseContext(options, fakeLoggerFactory))
            {
                context.Add(fakeUser1);
                context.Add(fakeUser2);
                context.Add(fakeEndpoint);
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
                Assert.Null(controllerResult.Value); ;
                context.Database.EnsureDeleted();
            }
        }

        //todo...
    }
}
