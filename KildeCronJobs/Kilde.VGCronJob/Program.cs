using Kilde.Application.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kilde.VGCronJob
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
                    options.ServiceName = "VGCronJob";
                })
                 .ConfigureServices(services =>
                 {
                     services.ConfigureApplicationServices();
                     services.AddHostedService<VGWebJob>();
                 })
                 .ConfigureLogging(logging =>
                 {
                     logging.AddConsole();
                     logging.AddEventLog(c => {
                         c.SourceName = "VGCronJob";
                         });
                 });
        }
        
    }
}