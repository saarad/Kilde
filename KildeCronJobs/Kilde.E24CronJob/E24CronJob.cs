using Kilde.Application.ApplicationServices;
using Kilde.Application.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KildeCronJobs.Common.Services;
using Kilde.E24CronJob;

namespace Kilde.E24CronJob
{
    public class E24CronJob : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public E24CronJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                while (true)
                {
                    using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                    using (var scope = _serviceProvider.CreateScope()) //keep unsmiplified to avoid dev errors
                    {
                        var logger = loggerFactory
                            .CreateLogger<Program>();
                        logger.LogInformation("Starting work on E24");

                        var articleService = _serviceProvider.GetRequiredService<IArticleService>();
                        var e24Service = new E24Service(logger, articleService);
                        await e24Service.DoWork();
                    }
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
