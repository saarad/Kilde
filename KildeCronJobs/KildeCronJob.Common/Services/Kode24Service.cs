using HtmlAgilityPack;
using Kilde.Application.ApplicationServices;
using Kilde.Application.Extensions;
using Kilde.Application.Models;
using KildeCronJobs.Common.Clients;
using KildeCronJobs.Common.Domain;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KildeCronJobs.Common.Services
{
    public class Kode24Service
    {
        private readonly ILogger<dynamic> _logger;
        private readonly IArticleService _articleService;
        private const string ApiKey = "";

        public Kode24Service(ILogger<dynamic> logger, IArticleService articleService)
        {
            _logger = logger;
            _articleService = articleService;
        }

        public async Task DoWork()
        {
            try
            {
                _logger.LogInformation("Cron Job Kode24 started execution at: {DateTime.Now}", DateTime.Now.ToString());
                var existingArticles = await _articleService.GetArticlesBySource("KODE24", takeAll: true);

                var lookup = existingArticles.ToDictionary(x => x.Identifier.ToLower());

                using var httpClient = new HttpClient();

                var response = await httpClient.GetAsync("https://docs.kode24.no/api/frontpage");
                var result = await response.Content.ReadAsStringAsync();

                var articles = JsonConvert.DeserializeObject<Kode24Root>(result);
                if (articles == null || !articles.LatestArticles.Any())
                    _logger.LogError("No articles in response from Kode24 API");

                var articlesToAdd = new List<Article>();

                foreach (var article in articles?.LatestArticles ?? new())
                {
                    var identifier = article.Id;
                    var existing = existingArticles.Where(x => x.Identifier.ToLower() == identifier.ToLower()).FirstOrDefault();

                    if(existing == null)
                    {
                        var updatedOn = article.Published.ToKildeFormat();
                        var title = article.Title;
                        var link = $"https://www.kode24.no{article.Published_Url}";
                        var kildeArticle = new Article
                        {
                            Identifier = identifier,
                            Title = title,
                            Description = article.Subtitle,
                            LastUpdated = updatedOn,
                            Link = link,
                            Source = "KODE24"
                        };

                        articlesToAdd.Add(kildeArticle);
                    }
                }
                var batches = articlesToAdd.Chunk(55);
                foreach (var batch in batches) //to avoid triggering paid gemini subscription
                {
                    await AddArticles(batch);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }

                _logger.LogInformation("Work completed for Kode24 at {time}", DateTime.Now.ToString());

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong on top level. Error: {ex}");
                Console.WriteLine("Exception " + ex.ToString());
            }
        }

        private async Task AddArticles(IEnumerable<Article> articles)
        {
            var geminiClient = new GeminiClient(ApiKey);
            using var httpClient = new HttpClient();
            foreach (var article in articles)
            {
                try
                {
                    var description = "Klikk på linken for å lese mer";

                    var articleResponse = await httpClient.GetAsync(article.Link);
                    var articleResult = await articleResponse.Content.ReadAsStringAsync();

                    var articleHtmlDocument = new HtmlDocument();
                    articleHtmlDocument.LoadHtml(articleResult);


                    var articleScriptTag = articleHtmlDocument.DocumentNode.ChildNodes
                        .Where(x => x.Name == "html")
                        .FirstOrDefault()?
                        .ChildNodes.Where(x => x.Name == "head")
                        .FirstOrDefault()?
                        .ChildNodes
                        .Where(x => x.Name == "script" && x.Attributes.Any(y => y.Value == "application/ld+json"))
                        .FirstOrDefault() ?? throw new Exception("Could not fetch json from tekno html");

                    var deseralized = JsonConvert.DeserializeObject<Kode24NewsArticle>(articleScriptTag.InnerText);

                    if (deseralized == null) //break soft if one article load fails
                        continue;

                    var summary = await geminiClient.GetGeminiSummary(deseralized.ArticleBody);

                    if (!string.IsNullOrWhiteSpace(summary))
                        description = summary;

                    var image = deseralized.Image.FirstOrDefault() ?? string.Empty;
                    var updatedOn = article.LastUpdated;

                    try
                    {
                        updatedOn = deseralized.DateModified.ToKildeFormat();
                    }
                    catch { }
                   

                    var mapped = new ArticleInputModel
                    {
                        Description = description,
                        Identifier = article.Identifier,
                        ImageLink = image,
                        Link = article.Link,
                        Title = article.Title,
                        LastUpdated = updatedOn ?? DateTime.Now.ToKildeFormat()
                    };

                    await _articleService.CreateArticle(mapped, "KODE24");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Something went wrong when adding {link}", article.Link);
                }
            }

        }
    }
}
