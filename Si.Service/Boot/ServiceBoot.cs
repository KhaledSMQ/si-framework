using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reactive.Linq;
using System.ServiceProcess;
using Common.Logging;
using Si.Common.Reactive;
using Si.Service.ServiceCommunication;
using Si.Service.Wrapper;

namespace Si.Service.Boot
{
    public interface IServiceBoot
    {
        void Boot();
        void Boot(IServiceArgs serviceArgs);
    }

    [Export(typeof(IServiceBoot))]
    public class ServiceBoot : IServiceBoot
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceBoot));

        private readonly IServiceInstance _serviceInstance;
        private readonly IEventPublisher _serviceEventPublisher;
        private readonly IEnvironmentWrapper _environment;

        [ImportingConstructor]
        public ServiceBoot
        (
            IServiceInstance serviceInstance,
            IEventPublisher serviceEventPublisher,
            IEnvironmentWrapper environmentWrapper
        )
        {
            _serviceInstance = serviceInstance;
            _serviceEventPublisher = serviceEventPublisher;
            _environment = environmentWrapper;
        }

        public void Boot()
        {
            Boot(null);
        }

        public void Boot(IServiceArgs serviceArgs)
        {
            if (serviceArgs != null)
            {
                if (serviceArgs.LaunchDebugger)
                {
                    Debugger.Launch();
                }
            }

            try
            {
                Log.Info("ServiceBoot starting Windows service instance.");

                _serviceEventPublisher.GetEvent<GracefulShutdownRequest>()
                                      .Take(1) // One is enough.
                                      .Subscribe(OnGracefulShutdownRequest);

                // Entry point when running in console mode.
                if (_environment.UserInteractive)
                {
                    SubscribeToConsoleCancel();

                    _serviceInstance.StartInstance();

                    Log.Info("Windows service instance started. Waiting for stop signal.");
                    // Wait on the stop signal of all instances of ServiceInstance. This will simulate the behaviour of ServiceBase.Run().
                    _serviceInstance.InstanceStopped.WaitOne();
                }
                // Entry point when running as a service.
                else
                {
                    var service = _serviceInstance as ServiceBase;
                    if (service == null)
                    {
                        Log.Warn($"Could not cast service instnace to type of ServiceBase. Instance: {_serviceInstance}.");
                    }
                    else
                    {
                        ServiceBase.Run(service);
                    }
                }

                Log.Info("Windows service instance has stopped.");
            }
            catch (Exception e)
            {
                Log.Error("ServiceBoot caught an unexpected exception while booting service.", e);
            }
        }

        private void OnGracefulShutdownRequest(GracefulShutdownRequest request)
        {
            try
            {
                Log.Info($"ServiceBoot recieved request from {request.Sender} to gracefully shutdown service. Reason: {request.Reason}");

                Log.Info("Stopping Windows service instance...");

                if (_environment.UserInteractive)
                {
                    _serviceInstance.StopInstance();
                }
                else
                {
                    var service = _serviceInstance as ServiceBase;
                    if (service == null)
                    {
                        Log.Warn($"Could not cast service instnace to type of ServiceBase. Instance: {_serviceInstance}.");
                    }
                    else
                    {
                        service.Stop();
                    }
                }

                Log.Info("Completed graceful shutdown of service.");

            }
            catch (Exception e)
            {
                Log.Error("ServiceBoot caught an unexpected exception while attempting to gracefully shutdown services.", e);
            }
        }

        private void SubscribeToConsoleCancel()
        {
            _environment.SubscribeToCancelKeyPress(OnConsoleCancel);
        }

        private void OnConsoleCancel(object sender, ConsoleCancelEventArgs args)
        {
            const string reason = "ServiceBoot recieved notification of console cancel key press.";
            Log.Info(reason);

            // Cancel event to prevent our app domain being killed before we can gracefully stop our processes.
            args.Cancel = true;

            OnGracefulShutdownRequest(new GracefulShutdownRequest(this, reason));
        }

    }
}
