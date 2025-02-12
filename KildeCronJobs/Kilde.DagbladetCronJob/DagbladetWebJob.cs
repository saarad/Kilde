using Kilde.Application.ApplicationServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using KildeCronJobs.Common.Services;

namespace Kilde.DagbladetCronJob
{
    public class DagBladetWebJob : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public DagBladetWebJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            while (true)
            {
                using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                using (var scope = _serviceProvider.CreateScope()) //keep unsmiplified to avoid dev errors
                {
                    var logger = loggerFactory
                        .CreateLogger<Program>();
                    logger.LogInformation("Starting work on Dagbladet");

                    var articleService = _serviceProvider.GetRequiredService<IArticleService>();
                    var dagBladet = new DagbladetService(logger, articleService);
                    await dagBladet.DoWork();
                }
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
