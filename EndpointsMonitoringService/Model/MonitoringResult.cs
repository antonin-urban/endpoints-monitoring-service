﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EndpointsMonitoringService.Model
{
    public class MonitoringResult
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTime DateOfCheck { get; set; }
        public int ReturnedHttpStatusCode { get; set; }
        public string ReturnedPayload { get; set; }
        public MonitoredEndpoint MonitoredEndpoint { get; set; }
        public int MonitoredEndpointForeignKey {get;set;}

        public MonitoringResult()
        {

        }
    }
}
