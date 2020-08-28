using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int UserForeignKey { get; set; }
        public User Owner { get; set; }

        public List<MonitoringResult> Results { get; set; }

        public MonitoredEndpoint()
        {

            this.DateOfCreation = DateTime.Now;

        }

    }
}
