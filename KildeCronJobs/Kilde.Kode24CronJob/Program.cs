using Kilde.Application.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kilde.Kode24CronJob
{
    internal class Program
    {
        static void Main(string[] args)
        {

            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                 .ConfigureServices(services =>
                 {
                     services.ConfigureApplicationServices();
                     services.AddHostedService<Kode24CronJob>();
                 })
                 .ConfigureLogging(logging =>
                 {
                     logging.AddConsole();
                     logging.AddEventLog(c =>
                     {
                         c.SourceName = "E24CronJob";
                     });
                 });
        }
    }
}