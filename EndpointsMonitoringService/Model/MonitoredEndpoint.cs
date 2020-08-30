using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EndpointsMonitoringService.Model
{
    public class MonitoredEndpoint
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime DateOfCreation { get; set; }
        public DateTime DateOfLastCheck { get; set; }
        public int MonitoredInterval { get; set; } //s
        [JsonIgnore]
        public int UserForeignKey { get; set; }
        [JsonIgnore]
        public User Owner { get; set; }
        [JsonIgnore]
        public List<MonitoringResult> Results { get; set; }

        public MonitoredEndpoint()
        {

            this.DateOfCreation = DateTime.Now;

        }

    }
}
