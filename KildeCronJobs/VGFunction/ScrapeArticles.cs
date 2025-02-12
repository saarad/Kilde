using System.Threading.Tasks;
using Kilde.Application.ApplicationServices;
using KildeCronJobs.Common.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;

namespace VGFunction
{
    public class ScrapeArticles
    {
        private readonly IArticleService _articleService;

        public ScrapeArticles(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [FunctionName("ScrapeArticles")]
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            await DoWork();
        }

        private async Task DoWork()
        {
            var mock = new Mock<ILogger<ScrapeArticles>>();
            var vgService = new VGService(mock.Object, _articleService);
            await vgService.DoWork();
        }
    }
}
