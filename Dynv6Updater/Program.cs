using Unity;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Dynv6Updater
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada de la aplicacion principal
        /// </summary>
        /// <remarks>
        /// DEBUG: para depurar el servicio como aplicacion
        /// RELEASE: para compilar el servicio y desplegarlo en servicios de windows
        /// </remarks>
        static void Main()
        {
            IUnityContainer unityContainer = UnityConfig.RegisterComponents();

#if DEBUG            
            Win32Service service = unityContainer.Resolve<Win32Service>();
            service.GetType().GetMethod("OnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(service, new object[] { new string[] { } });

            // esperar 2 minutos antes de finalizar la aplicacion (para simular el servicio en background)
            System.Threading.Thread.Sleep(1000 * 60 * 2);
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                unityContainer.Resolve<Win32Service>()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
