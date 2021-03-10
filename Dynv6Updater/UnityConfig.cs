using Dynv6Updater.Services;
using Dynv6Updater.Services.Models;
using Microsoft.Extensions.Options;
using System.Configuration;
using Unity;

namespace Dynv6Updater
{
    public static class UnityConfig
    {
        public static IUnityContainer RegisterComponents()
        {
            var container = new UnityContainer();

            // Dynv6Updater
            container.RegisterType<Win32Service, Win32Service>();

            // Dynv6Updater.Services
            container.RegisterType<IUpdaterService, UpdaterService>();
            container.RegisterFactory<IOptions<AppSettingsModel>>((unityContainer) =>
            {
                AppSettingsModel settings = new AppSettingsModel
                {
                    ProcessInterval = int.TryParse(ConfigurationManager.AppSettings["PROCESS_INTERVAL"], out int interval) ? interval : 10,
                    Dynv6HostName = ConfigurationManager.AppSettings["DYNV6_HOST_NAME"],
                    Dynv6HttpToken = ConfigurationManager.AppSettings["DYNV6_HTTP_TOKEN"]
                };

                return Options.Create(settings);
            });



            return container;
        }
    }
}
