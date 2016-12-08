using System.ServiceProcess;

namespace Si.Service.Boot
{
    public class ServiceConfiguration
    {
        public bool DelayedAutoStart { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string ServiceName { get; set; }
        public string[] ServicesDependedOn { get; set; }
        public ServiceStartMode StartType { get; set; }
        public ServiceAccount Account { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

    }
}
