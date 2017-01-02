using CommandLine;

namespace Si.Service.ServiceCommunication
{
    public interface IServiceArgs
    {
        bool LaunchDebugger { get; }
    }

    public class ServiceArgs : IServiceArgs
    {
        [Option('d', "debug", DefaultValue = false, Required = false, HelpText = "Indicates whether the service should launch the debugger on start.")]
        public bool LaunchDebugger { get; set; }
        
    }
}
