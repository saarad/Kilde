using System.Threading.Tasks;
using Kilde.Application.ApplicationServices;
using KildeCronJobs.Common.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kode24Function
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
            var kode24Service = new Kode24Service(mock.Object, _articleService);
            await kode24Service.DoWork();
        }
    }
}
