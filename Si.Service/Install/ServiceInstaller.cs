using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using CommandLine;
using Si.Logging;

namespace Si.Service.Install
{
    [DesignerCategory("Code")]
    [RunInstaller(true)]
    public class ServiceInstaller : System.Configuration.Install.Installer
    {
        protected override void OnBeforeInstall(IDictionary savedState)
        {
            SetUpInstallers();
            base.OnBeforeInstall(savedState);
        }

        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            SetUpInstallers();
            base.OnBeforeUninstall(savedState);
        }

        public void SetUpInstallers()
        {
            try
            {
                ServiceConfiguration configuration = GetServiceConfig();

                Context.Parameters["assemblypath"] = configuration.PathToExecutable;

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

        private ServiceConfiguration GetServiceConfig()
        {
            // Read arguments from context parameters.
            // Parameters should have been passed in in the following format (for example):
            // InstallUtil.exe /-e="C:\services\MyService\MyService.exe" /-n="MyServiceDisplayName" /-s="MyServiceServiceName" /--description="MyService service description." /-a="LocalService" /i Si.Service.dll

            var args = new List<string>();
            foreach (string key in Context.Parameters.Keys)
            {
                if (key.StartsWith("-"))
                {
                    args.Add(key);

                    var value = Context.Parameters[key];
                    if (!string.IsNullOrEmpty(value))
                    {
                        args.Add(value);
                    }
                }
            }
            
            var configuration = new ServiceConfiguration();
            Parser.Default.ParseArgumentsStrict(args.ToArray(), configuration);

            return configuration;
        }

    }
}
