using System;
using System.ComponentModel.Composition;
using System.ServiceProcess;
using System.Threading;
using Common.Logging;

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

        public ServiceInstance() { }
        [ImportingConstructor]
        public ServiceInstance
        (
            IServiceRunner serviceRunner
        )
        {
            _serviceRunner = serviceRunner;
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
        
    }
}
