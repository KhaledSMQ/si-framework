using System;
using Common.Logging;
using Si.Common.Core;
using Si.Service.Boot;

namespace Si.Service
{
    public static class ServiceEntry
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceEntry));

        public static void RunService() => RunService(new string[0]);

        public static void RunService(string[] args)
        {
            try
            {
                var compositionContainer = CompositionContainerFactory.Instance;
                var boot = compositionContainer.GetExportedValue<IServiceBoot>();

                boot.Boot(args);
            }
            catch (Exception e)
            {
                Log.Error($"Service could not be started. {e.Message}", e);
                throw;
            }
        }

    }
}
