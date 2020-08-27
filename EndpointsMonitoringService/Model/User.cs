using System;
using System.Collections.Generic;

namespace EndpointsMonitoringService.Model
{
    public class User
    {

        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string AccessToken { get; set; }

        public List<MonitoredEndpoint> Endpoints { get; set; }

        public User()
        {

        }


    }
}
