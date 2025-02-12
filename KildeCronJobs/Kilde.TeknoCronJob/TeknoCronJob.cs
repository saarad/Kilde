using Kilde.Application.ApplicationServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using KildeCronJobs.Common.Services;


namespace Kilde.TeknoCronJob
{
    public class TeknoCronJob : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public TeknoCronJob(IServiceProvider serviceProvider)
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
                    var articleService = scope.ServiceProvider.GetRequiredService<IArticleService>();
                    var teknoService = new TeknoService(logger, articleService);
                    await teknoService.DoWork();
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
