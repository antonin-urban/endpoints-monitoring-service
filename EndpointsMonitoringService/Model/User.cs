using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EndpointsMonitoringService.Model
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
