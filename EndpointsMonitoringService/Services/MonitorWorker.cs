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

namespace EndpointsMonitoringService.Services
{
    public class MonitorWorker : IHostedService, IDisposable
    {


        private System.Timers.Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonitorWorker> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public MonitorWorker(IServiceScopeFactory scopeFactory, IHttpClientFactory clientFactory)
        {
            _scopeFactory = scopeFactory;
            _clientFactory = clientFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Logger.ForContext(typeof(MonitorWorker)).Information("MonitorWorker started");

            _timer = new System.Timers.Timer(5000);
            _timer.AutoReset = true;
            _timer.Elapsed += _doWork;
            _timer.Start();

            return Task.CompletedTask;
        }

        private async void _doWork(object sender, ElapsedEventArgs e)
        {

            Log.Logger.ForContext(typeof(MonitorWorker)).Information("Monitor waking up");
            var endpoints = new List<MonitoredEndpoint>();
            var results = new List<MonitoringResult>();
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                var timestamp = DateTime.Now;
                foreach (var endpoint in dbContext.MonitoredEndpoint)
                {

                    if (endpoint.DateOfLastCheck != default)
                    {
                        var targetTime = endpoint.DateOfLastCheck.AddSeconds(endpoint.MonitoredInterval);
                        if (targetTime > timestamp)
                        {
                            continue;
                        }
                    }


                    endpoint.DateOfLastCheck = timestamp;
                    await dbContext.SaveChangesAsync();

                    MonitoringResult result = new MonitoringResult();
                    result.DateOfCheck = timestamp;
                    result.MonitoredEndpoint = endpoint;

                    try
                    {

                        var request = new HttpRequestMessage(HttpMethod.Get, endpoint.Url);

                        var client = _clientFactory.CreateClient();


                        try
                        {
                            var response = await client.SendAsync(request);


                            int statusCodeInt = default;
                            int.TryParse(response.StatusCode.ToString(), out statusCodeInt);
                            result.ReturnedHttpStatusCode = statusCodeInt;

                            if (response.IsSuccessStatusCode)
                            {
                                try
                                {
                                    using var responseString = response.Content.ReadAsStringAsync();
                                    result.ReturnedPayload = MySql.Data.MySqlClient.MySqlHelper.EscapeString(responseString.Result);
                                }
                                catch(Exception exx)
                                {
                                    result.ReturnedPayload = "ERROR READING RETURNED PAYLOAD AS STRING: " + MySql.Data.MySqlClient.MySqlHelper.EscapeString(exx.Message);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            result.ReturnedPayload= "SERVICE EXCEPTION: " + MySql.Data.MySqlClient.MySqlHelper.EscapeString(ex.Message);
                        }



                    }
                    catch (Exception ex)
                    {
                        Log.Logger.ForContext(typeof(MonitorWorker)).Error(ex, String.Format("ERROR SENDING REQUEST TO ENDPOINT ID {0}", endpoint.Id));
                    }
                }
            }

        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Logger.ForContext(typeof(MonitorWorker)).Information("MonitorWorker stopped");
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
