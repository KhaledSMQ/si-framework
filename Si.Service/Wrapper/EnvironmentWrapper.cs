using System;
using System.ComponentModel.Composition;

namespace Si.Service.Wrapper
{
    public interface IEnvironmentWrapper
    {
        bool UserInteractive { get; }
        void SubscribeToCancelKeyPress(ConsoleCancelEventHandler handler);
    }
    
    [Export(typeof(IEnvironmentWrapper))]
    public class EnvironmentWrapper : IEnvironmentWrapper
    {
        public bool UserInteractive => Environment.UserInteractive;

        public void SubscribeToCancelKeyPress(ConsoleCancelEventHandler handler)
        {
            Console.CancelKeyPress += handler;
        }

    }
}
