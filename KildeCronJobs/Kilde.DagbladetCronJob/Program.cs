using Kilde.Application.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kilde.DagbladetCronJob
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
                    options.ServiceName = "DagbladetCronJob";
                })
                 .ConfigureServices(services =>
                 {
                     services.ConfigureApplicationServices();
                     services.AddHostedService<DagBladetWebJob>();
                 })
                 .ConfigureLogging(logging =>
                 {
                     logging.AddConsole();
                     logging.AddEventLog(c =>
                     {
                         c.SourceName = "DagbladetCronJob";
                     });
                 });
        }

    }
}