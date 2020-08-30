using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EndpointsMonitoringService.Model
{
    public class MonitoringResult
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTime DateOfCheck { get; set; }
        public int ReturnedHttpStatusCode { get; set; }
        public string ReturnedPayload { get; set; }
        [JsonIgnore]
        public MonitoredEndpoint MonitoredEndpoint { get; set; }
        [DisplayName("MonitoredEndopoint")]
        public int MonitoredEndpointForeignKey {get;set;}

        public MonitoringResult()
        {

        }
    }
}
