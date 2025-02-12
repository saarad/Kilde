using Kilde.Application.ApplicationServices;
using Kilde.Application.Models;
using KildeCronJobs.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kilde.VGCronJob
{
    public class VGWebJob : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public VGWebJob(IServiceProvider serviceProvider)
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
                    logger.LogInformation("Starting work on VG");

                    var articleService = _serviceProvider.GetRequiredService<IArticleService>();
                    var vgService = new VGService(logger, articleService);
                    await vgService.DoWork();
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
