using System;
using System.ServiceProcess;
using Si.Logging;

namespace Si.Service.Boot
{
    [System.ComponentModel.DesignerCategory("Code")]
    public partial class ServiceInstaller : System.Configuration.Install.Installer
    {
        protected void Install(ServiceConfiguration configuration)
        {
            try
            {
                var serviceInstaller = new System.ServiceProcess.ServiceInstaller
                {
                    DelayedAutoStart = configuration.DelayedAutoStart,
                    Description = configuration.Description,
                    DisplayName = configuration.DisplayName,
                    ServiceName = configuration.ServiceName,
                    ServicesDependedOn = configuration.ServicesDependedOn,
                    StartType = configuration.StartType,
                };

                var serviceProcessInstaller = new ServiceProcessInstaller
                {
                    Account = configuration.Account,
                    Username = configuration.Username,
                    Password = configuration.Password,
                };

                Installers.Add(serviceInstaller);
                Installers.Add(serviceProcessInstaller);
            }
            catch (Exception e)
            {
                var emergencyLogger = EmergencyLog.Instance;
                emergencyLogger.Log(LogLevel.Error, "ServiceInstaller caught an unexpected exception while attempting to create installers.", e);
                throw;
            }
        }

    }
}
