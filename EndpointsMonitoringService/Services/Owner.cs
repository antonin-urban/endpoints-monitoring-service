using System;
namespace EndpointsMonitoringService.Model
{
    public class Owner : IOwner
    {
        public User Data { get; private set; }

        public Owner()
        {

        }

        public void RegisterOwner(User user)
        {
            Data = user;
        }
    }
}
