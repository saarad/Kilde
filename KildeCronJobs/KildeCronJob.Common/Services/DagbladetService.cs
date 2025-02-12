using HtmlAgilityPack;
using Kilde.Application.ApplicationServices;
using Kilde.Application.Extensions;
using Kilde.Application.Models;
using KildeCronJobs.Common.Clients;
using KildeCronJobs.Common.Domain;
using KildeCronJobs.Common.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KildeCronJobs.Common.Services
{
    public class DagbladetService
    {
        private readonly ILogger<dynamic> _logger;
        private readonly IArticleService _articleService;
        private const string ApiKey = "";

        public DagbladetService(ILogger<dynamic> logger, IArticleService articleService)
        {
            _logger = logger;
            _articleService = articleService;
        }

        public async Task DoWork()
        {
            try
            {
                _logger.LogInformation("Cron Job Dagbladet started execution at: {DateTime.Now}", DateTime.Now.ToString());

                var existingArticles = await _articleService.GetArticlesBySource("DAGBLADET", takeAll: true);
                var lookup = existingArticles.ToDictionary(x => x.Identifier.ToLower());

                using var httpClient = new HttpClient();

                var response = await httpClient.GetAsync("https://www.dagbladet.no/");
                var result = await response.Content.ReadAsStringAsync();

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(result);

                var main = htmlDocument.DocumentNode.SelectSingleNode("//main");
                var articles = main.SelectNodes("//article");
                var articlesToAdd = new List<Article>();

                foreach(var element in articles)
                {
                    var linkElement = element.ChildNodes.Where(x => x.Name == "a").FirstOrDefault();
                    if(linkElement == null)
                    {
                        _logger.LogWarning("Link element missing from article");
                        continue;
                    }

                    var link = linkElement.GetAttributeValue("href", "");
                    if (!link.Contains("dagbladet.no") 
                        || link.StartsWith("https://www.dagbladet.no/video/") //ignore videos
                        || link.StartsWith("https://www.dagbladet.no/tema") //ignore pluss articles
                        || link.StartsWith("https://www.dagbladet.no/spesial") //ignore pluss articles 
                        || link.StartsWith("https://www.dagbladet.no/studio")) //ignore news stream
                        continue;

                    var identifier = link.DagbladetIdentifier();
                    if(identifier == null)
                    {
                        _logger.LogError("Could not find identifier in link {link}", link);
                        continue;
                    }

                    var existing = lookup.GetValueOrDefault(identifier.ToLower());

                    if(existing == null)
                    {
                        var headline = linkElement
                            .ChildNodes
                            .Where(x => x.Name == "header")
                            .FirstOrDefault()
                            ?.ChildNodes
                            .Where(x => x.Name == "h3")
                            .FirstOrDefault()
                            ?.InnerText;

                        headline = System.Web.HttpUtility.HtmlDecode(headline);

                        var image = linkElement.
                            ChildNodes
                            .Where(x => x.Name == "figure")
                            .FirstOrDefault()
                            ?.ChildNodes
                            .Where(x => x.Name == "picture")
                            .FirstOrDefault()
                            ?.ChildNodes
                            .Where(x => x.Name == "img")
                            .FirstOrDefault()
                            ?.GetAttributeValue("src", "");

                        var article = new Article
                        {
                            Title = headline ?? string.Empty,
                            Identifier = identifier,
                            ImageLink = image ?? string.Empty,
                            Link = link,
                            Source = "DAGBLADET"
                        };

                        articlesToAdd.Add(article);
                    }
                }
                var batches = articlesToAdd.Chunk(55);
                foreach(var batch in batches)
                {
                    await AddArticles(batch);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong on top level. Error: {ex}");
                Console.WriteLine("Exception " + ex.ToString());
            }

            _logger.LogInformation("Work completed for Dagbladet");
        }

        private async Task AddArticles(IEnumerable<Article> articles)
        {
            var geminiClient = new GeminiClient(ApiKey);
            foreach (var article in articles)
            {
                try
                {
                    using var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromMinutes(3);
                    var response = await httpClient.GetAsync(article.Link);
                    var result = await response.Content.ReadAsStringAsync();


                    var htmlLoader = new HtmlDocument();
                    htmlLoader.LoadHtml(result);

                    var articleScriptTag = htmlLoader.DocumentNode.ChildNodes
                        .Where(x => x.Name == "html")
                        .FirstOrDefault()?
                        .ChildNodes.Where(x => x.Name == "head")
                        .FirstOrDefault()?
                        .ChildNodes
                        .Where(x => x.Name == "script" && x.Attributes.Any(y => y.Value == "application/ld+json"))
                        .FirstOrDefault() ?? throw new Exception("Could not fetch json from dagbladet html");

                    var newArticle = JsonConvert.DeserializeObject<DagbladetNewsArticle>(articleScriptTag.InnerText);

                    var headline = newArticle?.Headline ?? article.Title;
                    headline = System.Web.HttpUtility.HtmlDecode(headline);

                    var description = "Klikk på linken for å lese mer";
                    var image = newArticle?.Image.LastOrDefault()?.Url ?? article.ImageLink;

                    try //attempt to get data from article, save with existing metadata on failure
                    {
                        var text = newArticle?.ArticleBody;
                        if (text != null)
                            description = await geminiClient.GetGeminiSummary(text);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Could not scrape description for link {article.Link}");

                    }

                    string? updatedOn = default;

                    try
                    {
                        updatedOn = newArticle?.DateModified.ToKildeFormat();

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Could not set updated on for on {article.Link}.");
                    }

                    var mapped = new ArticleInputModel
                    {
                        Description = description,
                        Identifier = article.Identifier,
                        ImageLink = image,
                        Link = article.Link,
                        Title = article.Title,
                        LastUpdated = updatedOn ?? DateTime.Now.ToKildeFormat()
                    };

                    await _articleService.CreateArticle(mapped, "DAGBLADET");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Something went wrong for {article}", article.Link);
                }
            }
        }
    }
}
