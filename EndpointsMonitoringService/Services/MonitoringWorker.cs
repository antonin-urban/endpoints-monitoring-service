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
                endpoints = await dbContext.MonitoredEndpoint.ToListAsync();
            }

            foreach (var endpoint in endpoints)
            {
                var timeStamp = DateTime.Now;

                if (!TimeTest(endpoint, timeStamp))
                {
                    continue;
                }

                endpoint.DateOfLastCheck = timeStamp;

                MonitoringResult result = new MonitoringResult();
                result.DateOfCheck = timeStamp;
                result.MonitoredEndpoint = endpoint;

                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, new UriBuilder(endpoint.Url).Uri);
                    var client = _clientFactory.CreateClient();

                    try
                    {
                        var response = await client.SendAsync(request);
                        int statusCodeInt = ResolveStatusCode(response);
                        result.ReturnedPayload = await ExtractPayloadAsync(response);

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

                await SaveMonitoringResultAsync(endpoint, result);
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

        private bool TimeTest(MonitoredEndpoint endpoint, DateTime timeStamp)
        {
            var result = true;
            if (endpoint.DateOfLastCheck != default)
            {
                var targetTime = endpoint.DateOfLastCheck.AddSeconds(endpoint.MonitoredInterval);
                if (targetTime > timeStamp)
                {
                    result = false;
                }
            }
            return result;
        }

        private async Task<bool> SaveMonitoringResultAsync(MonitoredEndpoint endpoint, MonitoringResult monitoringResult)
        {
            var result = false;

            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    dbContext.MonitoredEndpoint.Update(endpoint);
                    dbContext.MonitoringResult.Add(monitoringResult);
                    await dbContext.SaveChangesAsync();
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.ForContext(typeof(MonitoringWorker)).Error(ex, String.Format("ERROR SAVING CHANGES TO DB FOR ENDPOINT ID {0}", endpoint.Id));
            }
            return result;
        }

        private async Task<string> ExtractPayloadAsync(HttpResponseMessage response)
        {
            string payload;

            if (response.Content == null)
            {
                return default;
            }

            try
            {
                payload = await response.Content.ReadAsStringAsync();

                MySql.Data.MySqlClient.MySqlHelper.EscapeString(payload);
                if (payload.Length > 20000)
                {
                    payload = payload.Substring(0, 20000).ToString();
                };

                return payload;
            }
            catch (Exception exx)
            {
                payload = "ERROR READING RETURNED PAYLOAD AS STRING: " + MySql.Data.MySqlClient.MySqlHelper.EscapeString(exx.Message);
            }

            return payload;
        }

        private int ResolveStatusCode(HttpResponseMessage response)
        {
            int statusCodeInt = default;
            int.TryParse(response.StatusCode.ToString(), out statusCodeInt);
            return statusCodeInt;
        }
    }
}
