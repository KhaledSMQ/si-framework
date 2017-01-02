using System.ServiceProcess;
using CommandLine;

namespace Si.Service.Install
{
    public class ServiceConfiguration
    {
        [Option('e', "path-to-exe", Required = true, HelpText = "Absolute path to the service executable file.")]
        public string PathToExecutable { get; set; }

        [Option('n', "display-name", Required = true, HelpText = "Indicates the friendly name that identifies the service to the user.")]
        public string DisplayName { get; set; }

        [Option('s', "service-name", Required = true, HelpText = "Indicates the name used by the system to identify this service. This property must be identical to the System.ServiceProcess.ServiceBase.ServiceName of the service you want to install.")]
        public string ServiceName { get; set; }

        [Option('d', "description", DefaultValue = "", Required = false, HelpText = "Description for the service.")]
        public string Description { get; set; }

        [OptionArray("service-depended-on", DefaultValue = new string[0], Required = false, HelpText = "Indicates the services that must be running for this service to run.")]
        public string[] ServicesDependedOn { get; set; }

        [Option("delayed-autostart", DefaultValue = false, Required = false, HelpText = "Indicates whether the service should be delayed from starting until other automatically started services are running.")]
        public bool DelayedAutoStart { get; set; }

        [Option("start-type", DefaultValue = ServiceStartMode.Manual, Required = false, HelpText = "A System.ServiceProcess.ServiceStartMode that represents the way the service is started. The default is Manual, which specifies that the service will not automatically start after reboot.")]
        public ServiceStartMode StartType { get; set; }

        [Option('a', "account", DefaultValue = ServiceAccount.User, Required = false, HelpText = "A System.ServiceProcess.ServiceAccount that defines the type of account under which the system runs this service.")]
        public ServiceAccount Account { get; set; }

        [Option('u', "username", DefaultValue = null, Required = false, HelpText = "The account under which the service should run. Required if Account is User.")]
        public string Username { get; set; }

        [Option('p', "password", DefaultValue = null, Required = false, HelpText = "The password associated with the account under which the service should run. Required if Account is User.")]
        public string Password { get; set; }

    }
}
