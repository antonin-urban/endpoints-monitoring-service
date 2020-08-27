using System;
namespace EndpointsMonitoringService.Model
{
    public class MonitoredEndpoint
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime DateOfCreation { get; set; }
        public DateTime DateOfLastCheck { get; set; }
        public int MonitoredInterval { get; set; } //ms
        public int UserForeignKey { get; set; }
        public User Owner { get; set; }


        public MonitoredEndpoint()
        {

            this.DateOfCreation = DateTime.Now;

        }

    }
}
