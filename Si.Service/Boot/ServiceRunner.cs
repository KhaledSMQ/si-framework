using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;

namespace Si.Service.Boot
{
    public interface IServiceRunner
    {
        Task RunAsync();
        Task GracefulShutdownAsync();
    }

    [Export(typeof(IServiceRunner))]
    public class ServiceRunner : IServiceRunner
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceRunner));
        
        private readonly List<IService> _services;

        [ImportingConstructor]
        public ServiceRunner
        (
            [ImportMany] IEnumerable<IService> services
        )
        {
            _services = services.ToList();
        }

        public Task RunAsync()
        {
            if (!_services.Any())
            {
                Log.Warn($"{nameof(ServiceRunner)} did not find any instances of {nameof(IService)}.");
                return Task.CompletedTask;
            }
            
            Log.Info("Starting services.");

            return Task.WhenAll(
                _services.Select(s => ActOnServiceAsync(s, x => x.StartAsync(), "Start Service"))
                    .ToArray());
        }

        public Task GracefulShutdownAsync()
        {
            Log.Info("Stopping services.");

            return Task.WhenAll(
                _services.Select(s => ActOnServiceAsync(s, x => x.StopAsync(), "Stop Service"))
                    .ToArray());
        }

        private static Task ActOnServiceAsync(
            IService service,
            Func<IService, Task> act,
            string description)
        {
            var type = service.GetType().Name;

            Log.Info($"Processing action '{description}' on {type}...");
            return act(service)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        var errorMessage = $"Error performing action '{description}' on {type}.";
                        var error = t.Exception?.Flatten();
                        if (error == null)
                        {
                            Log.Error(errorMessage);
                        }
                        else
                        {
                            Log.Error(errorMessage, error);
                        }
                    }
                    else
                    {
                        Log.Info($"Completed action '{description}' on {type}.");
                    }
                });
        }

    }
}
