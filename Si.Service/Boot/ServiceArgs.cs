using System.Linq;

namespace Si.Service.Boot
{
    public interface IServiceArgs
    {
        bool LaunchDebugger { get; }
    }

    public class ServiceArgs : IServiceArgs
    {
        public ServiceArgs(string[] args)
        {
            if (args == null || !args.Any())
            {
                AssignDefaultValues();
                return;
            }

            ParseArgs(args);
        }

        public bool LaunchDebugger { get; private set; }

        private void AssignDefaultValues()
        {
            LaunchDebugger = false; // Not really required, but in the future we might have more complicated service args.
        }

        private void ParseArgs(string[] args)
        {
            if (args.Contains("-d")) LaunchDebugger = true;
        }

    }
}
