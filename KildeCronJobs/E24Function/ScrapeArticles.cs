using System.Threading.Tasks;
using Kilde.Application.ApplicationServices;
using KildeCronJobs.Common.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;

namespace E24Function
{
    public class ScrapeArticles
    {
        private readonly IArticleService _articleService;

        public ScrapeArticles(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [FunctionName("ScrapeArticles")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            await DoWork();
        }

        private async Task DoWork()
        {
            var mock = new Mock<ILogger<ScrapeArticles>>();
            var e24Service = new E24Service(mock.Object, _articleService);
            await e24Service.DoWork();
        }
    }
}
