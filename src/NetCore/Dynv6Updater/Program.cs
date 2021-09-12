using Dynv6Updater.Services;
using System;
using System.Threading.Tasks;

namespace Dynv6Updater
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Execute();
        }

        private static async Task Execute()
        {
            try
            {
                Console.WriteLine("Actualizando IP ....");

                IUpdateService updateService = new UpdaterService();
                await _updaterService.UpdateIp();

                Console.WriteLine("IP actualizada");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
