using System.ComponentModel.Composition;
using CommandLine;
using Si.Common.Core;

namespace Si.Service.ServiceCommunication
{
    public interface IServiceArgsRepo
    {
        void SetServiceArgs(string[] args);
        IServiceArgs GetServiceArgs();
    }

    [Export(typeof(IServiceArgsRepo))]
    public class ServiceArgsRepo : IServiceArgsRepo
    {
        private readonly object _argsLock;
        private readonly OneTimeFunc<string[], IServiceArgs> _parseArgs;

        private string[] _args;

        [ImportingConstructor]
        public ServiceArgsRepo()
        {
            _argsLock = new object();
            _parseArgs = new OneTimeFunc<string[], IServiceArgs>(DoParseArgs);
        }

        public void SetServiceArgs(string[] args)
        {
            if (_args != null)
            {
                return;
            }
            lock (_argsLock)
            {
                if (_args != null)
                {
                    return;
                }
                _args = args;
            }
        }

        public IServiceArgs GetServiceArgs()
        {
            if (_args == null)
            {
                SetServiceArgs(new string[0]);
            }

            return _parseArgs.Execute(_args);
        }

        private static IServiceArgs DoParseArgs(string[] args)
        {
            var serviceArgs = new ServiceArgs();
            Parser.Default.ParseArgumentsStrict(args, serviceArgs);

            return serviceArgs;
        }

    }
}