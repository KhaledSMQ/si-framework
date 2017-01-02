using System;
using System.ComponentModel.Composition;
using System.ServiceProcess;
using System.Threading;
using Common.Logging;
using Si.Common.Configuration;
using Si.Service.Wrapper;

namespace Si.Service.Boot
{
    public interface IServiceInstance
    {
        ManualResetEvent InstanceStopped { get; }

        void StartInstance();
        void StopInstance();
    }

    [System.ComponentModel.DesignerCategory("Code")]
    [Export(typeof(IServiceInstance))]
    public class ServiceInstance : ServiceBase, IServiceInstance
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceInstance));

        private readonly IServiceRunner _serviceRunner;
        
        [ImportingConstructor]
        public ServiceInstance
        (
            IConfiguration config,
            IEnvironmentWrapper environment,
            IServiceRunner serviceRunner
        )
        {
            _serviceRunner = serviceRunner;

            this.ServiceName = GetServiceName(config, environment);
            InstanceStopped = new ManualResetEvent(false);
        }

        public ManualResetEvent InstanceStopped { get; }

        protected override void OnStart(string[] args)
        {
            StartInstance();
        }

        protected override void OnStop()
        {
            StopInstance();
        }

        protected override void OnShutdown()
        {
            StopInstance();
        }

        public void StartInstance()
        {
            try
            {
                Log.Info("Starting service runner...");
                _serviceRunner.RunAsync()
                    .Wait();
                Log.Info("Service runner started");
            }
            catch (Exception e)
            {
                Log.Error("ServiceInstance caught an unexpected exception on start.", e);
            }
        }

        public void StopInstance()
        {
            try
            {
                Log.Info("Service runner stopping...");
                _serviceRunner.GracefulShutdownAsync()
                    .Wait();
                Log.Info("Service runner stopped.");
            }
            catch (Exception e)
            {
                Log.Error("ServiceInstance caught an unexpected exception on stop.", e);
            }
            finally
            {
                InstanceStopped.Set();
            }
        }

        private static string GetServiceName(IConfiguration config, IEnvironmentWrapper environment)
        {
            string serviceName;

            if (environment.UserInteractive)
            {
                if (!config.TryGetConfigValue(CoreConfigKey.ServiceName, "DebuggingService", out serviceName))
                {
                    // Allow no service name when debugging, but log a warning so the developer knows they need to set this before running as an installed service.
                    Log.Warn($"Application configuration file does not contain a value for key '{CoreConfigKey.ServiceName}'. This will be required when running as an installed service. When running as an installed service, this value must be identical to the service name the service was installed under.");
                }
            }
            else
            {
                serviceName = config.GetConfigValueOrError(CoreConfigKey.ServiceName, "A Windows service must have a service name to run. This name must be identical to the name the service was installed under.");
            }

            return serviceName;
        }

    }
}
