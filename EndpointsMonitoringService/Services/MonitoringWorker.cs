using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using EndpointsMonitoringService.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using System.Net.Http;
using EndpointsMonitoringService.Controllers;
using Org.BouncyCastle.Math.EC;

namespace EndpointsMonitoringService.Services
{
    public class MonitoringWorker : IHostedService, IDisposable
    {
        private System.Timers.Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonitoringWorker> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public MonitoringWorker(IServiceScopeFactory scopeFactory, IHttpClientFactory clientFactory)
        {
            _scopeFactory = scopeFactory;
            _clientFactory = clientFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Logger.ForContext(typeof(MonitoringWorker)).Information("MonitoringWorker started");

            _timer = new System.Timers.Timer(5000);
            _timer.AutoReset = true;
            _timer.Elapsed += _doWork;
            _timer.Start();

            return Task.CompletedTask;
        }

        private async void _doWork(object sender, ElapsedEventArgs e)
        {
            Log.Logger.ForContext(typeof(MonitoringWorker)).Information("MonitoringWorker waking up...");
            var endpoints = new List<MonitoredEndpoint>();
            //var results = new List<MonitoringResult>();

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                endpoints = dbContext.MonitoredEndpoint.ToList();
            }

            foreach (var endpoint in endpoints)
            {
                var timestamp = DateTime.Now;
                if (endpoint.DateOfLastCheck != default)
                {
                    var targetTime = endpoint.DateOfLastCheck.AddSeconds(endpoint.MonitoredInterval);
                    if (targetTime > timestamp)
                    {
                        continue;
                    }
                }

                endpoint.DateOfLastCheck = timestamp;
                MonitoringResult result = new MonitoringResult();
                result.DateOfCheck = timestamp;
                result.MonitoredEndpoint = endpoint;

                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, new UriBuilder(endpoint.Url).Uri);
                    var client = _clientFactory.CreateClient();

                    try
                    {
                        var response = await client.SendAsync(request);
                        int statusCodeInt = default;
                        int.TryParse(response.StatusCode.ToString(), out statusCodeInt);
                        result.ReturnedHttpStatusCode = statusCodeInt;

                        if (response.Content != null)
                        {
                            try
                            {
                                using var responseString = response.Content.ReadAsStringAsync();
                                result.ReturnedPayload = MySql.Data.MySqlClient.MySqlHelper.EscapeString(responseString.Result);
                                if (result.ReturnedPayload.Length > 20000)
                                {
                                    result.ReturnedPayload = result.ReturnedPayload.Substring(0, 20000).ToString();
                                };
                            }
                            catch (Exception exx)
                            {
                                result.ReturnedPayload = "ERROR READING RETURNED PAYLOAD AS STRING: " + MySql.Data.MySqlClient.MySqlHelper.EscapeString(exx.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.ReturnedPayload = "SERVICE EXCEPTION: " + MySql.Data.MySqlClient.MySqlHelper.EscapeString(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.ForContext(typeof(MonitoringWorker)).Error(ex, String.Format("ERROR SENDING REQUEST TO ENDPOINT ID {0}", endpoint.Id));
                }

                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                        dbContext.MonitoredEndpoint.Update(endpoint);
                        dbContext.MonitoringResult.Add(result);
                        await dbContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.ForContext(typeof(MonitoringWorker)).Error(ex, String.Format("ERROR SAVING CHANGES TO DB FOR ENDPOINT ID {0}", endpoint.Id));
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Logger.ForContext(typeof(MonitoringWorker)).Information("MonitoringWorker stopped");
            _timer.Stop();
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
