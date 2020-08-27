using System;
namespace EndpointsMonitoringService.Model
{
    public class MonitoringResult
    {
        public Int64 Id { get; set; }
        public DateTime DateOfCheck { get; set; }
        public int ReturnedHttpStatusCode { get; set; }
        public string ReturnedPayload { get; set; }
        public MonitoredEndpoint MonitoredEndpointId { get; set; }

        public MonitoringResult()
        {

        }
    }
}
