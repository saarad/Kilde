using Kilde.Application.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace Kilde.TeknoCronJob
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
                .UseWindowsService(options => 
                {
                    options.ServiceName = "TeknoCronJob";
                })
                 .ConfigureServices(services =>
                 {
                     services.ConfigureApplicationServices();
                     services.AddHostedService<TeknoCronJob>();
                 })
                 .ConfigureLogging(logging =>
                 {
                     logging.AddConsole();
                     logging.AddEventLog(c =>
                     {
                         c.SourceName = "TeknoCronJob";
                     });
                 });
        }
    }
}