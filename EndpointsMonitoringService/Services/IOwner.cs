using System;
namespace EndpointsMonitoringService.Model
{
    public interface IOwner
    {
        public User Data { get; }

        public void RegisterOwner(User user);


    }
}
