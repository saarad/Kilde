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
using static System.Formats.Asn1.AsnWriter;

namespace KildeCronJobs.Common.Services
{
    public class TeknoService
    {
        private const string ApiKey = "";
        private readonly IArticleService _articleService;
        private readonly ILogger<dynamic> _logger;

        public TeknoService(ILogger<dynamic> logger, IArticleService articleService)
        {
            _articleService = articleService;
            _logger = logger;
        }

        public async Task DoWork()
        {
            _logger.LogInformation("Starting work on TEKNO");
            try
            {

                _logger.LogInformation($"Cron Job Tekno executed at: {DateTime.Now}");

                var existingArticles = await _articleService.GetArticlesBySource("TEK", takeAll: true);
                var lookup = existingArticles.ToDictionary(x => x.Identifier);

                var linksToAdd = new List<Article>();
                var linksToUpdate = new List<Article>();

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync("https://www.tek.no/nyheter"); //"www" must be added for json script tag to be included in response
                var result = await response.Content.ReadAsStringAsync();

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(result);

                var scriptTag = htmlDocument.DocumentNode.ChildNodes
                    .Where(x => x.Name == "html")
                    .FirstOrDefault()?
                    .ChildNodes.Where(x => x.Name == "head")
                    .FirstOrDefault()?
                    .ChildNodes
                    .Where(x => x.Name == "script" && x.Attributes.Any(y => y.Value == "application/ld+json"))
                    .FirstOrDefault() ?? throw new Exception("Could not fetch json from tekno html");

                var roots = JsonConvert.DeserializeObject<List<TeknoRoot>>(scriptTag.InnerText);

                var itemLists = roots?.Where(x => x.NumberOfItems > 0).SelectMany(x => x.ItemListElement)
                    ?? throw new Exception("Could not deserialize tekno root to find list of articles");

                var articles = itemLists.Select(x => x.Url).ToList();
                
                foreach(var link in articles)
                {
                    //finding identifier
                    var indexOfLastSlash = link.LastIndexOf("/");
                    var linkWithoutTitle = link.Substring(0, indexOfLastSlash);
                    var indexOfIdentifier = linkWithoutTitle.LastIndexOf("/");
                    var identifier = linkWithoutTitle.Substring(indexOfIdentifier + 1);
                    
                    var existing = existingArticles.Where(x => x.Identifier.ToLower() == identifier.ToLower()).FirstOrDefault();

                    if(existing == null)
                    {
                        var articleResponse = await httpClient.GetAsync(link);
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

                        var articleTag = articleHtmlDocument.DocumentNode.SelectSingleNode("//article");
                        var text = string.Empty;
                        var paragrahps = articleTag.SelectNodes("//p");
                        paragrahps.ToList().ForEach(x => text += x.InnerText);

                        var articleRoot = JsonConvert.DeserializeObject<List<TeknoNewsArticle>>(articleScriptTag.InnerText);
                        var article = articleRoot?.Where(x => x.IsAccessibleForFree).FirstOrDefault();
                        if (article == null)
                            continue;

                        var imageLink = article.Image.LastOrDefault()?.Url ?? string.Empty;
                        var headline = string.IsNullOrEmpty(article.Headline) ? article.Name : article.Headline;
                        var toAdd = new Article
                        {
                            Description = text,
                            Identifier = identifier,
                            ImageLink = imageLink,
                            LastUpdated = article.DateModified.ToKildeFormat(),
                            Link = link,
                            Source = "TEK",
                            Title = headline
                        };

                        linksToAdd.Add(toAdd);
                    }

                    else
                    {
                        linksToUpdate.Add(existing);
                    }
                }

                var batches = linksToAdd.Chunk(55);
                foreach(var batch in batches) //to avoid triggering paid gemini subscription
                {
                    await AddArticles(batch);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }

                //await UpdateArticles(linksToUpdate, driver, _articleService);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong on top level. Error: {ex}");
                Console.WriteLine("Exception " + ex.ToString());
            }

            _logger.LogInformation("Work completed for TEKNO.");
        }
        private async Task AddArticles(IEnumerable<Article> articles)
        {
            var geminiClient = new GeminiClient(ApiKey);
            foreach (var article in articles)
            {
                try
                {
                    var description = "Klikk på linken for å lese mer";
                    var summary = await geminiClient.GetGeminiSummary(article.Description);
                    if (!string.IsNullOrWhiteSpace(summary))
                        description = summary;

                    var image = article.ImageLink;
                    string? updatedOn = article.LastUpdated;

                    var mapped = new ArticleInputModel
                    {
                        Description = description,
                        Identifier = article.Identifier,
                        ImageLink = article.ImageLink,
                        Link = article.Link,
                        Title = article.Title,
                        LastUpdated = updatedOn ?? DateTime.Now.ToKildeFormat()
                    };

                    await _articleService.CreateArticle(mapped, "TEK");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Something went wrong when adding {link}", article.Link);
                }
            }

        }


        //private async Task UpdateArticles(IEnumerable<Article> articles, IWebDriver currentDriver, IArticleService articleService)
        //{
        //    foreach (var article in articles)
        //    {
        //        try
        //        {

        //            await Task.Delay(2000);
        //            currentDriver.Navigate().GoToUrl(article.Link);

        //            var headline = article.Title;
        //            var description = article.Description;
        //            var image = article.ImageLink;

        //            string? updatedOn = default;

        //            try
        //            {
        //                var main = currentDriver.FindElement(By.TagName("main"));
        //                var articleTag = main.FindElement(By.TagName("article"));

        //                var timeTag = articleTag.FindElement(By.CssSelector("[aria-label='Publisert']")).GetAttribute("datetime");
        //                updatedOn = DateTime.Parse(timeTag).ToUniversalTime().ToString("u").Replace(" ", "T");

        //                if (updatedOn != article.LastUpdated)
        //                {
        //                    article.LastUpdated = updatedOn;
        //                    try //attempt to get new data from article, save with existing metadata on failure
        //                    {
        //                        var leadText = articleTag.FindElement(By.CssSelector("[data-test-tag='lead-text']")).Text;

        //                        description = !string.IsNullOrWhiteSpace(leadText) ? leadText : description;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Console.WriteLine($"Could not scrape description for link {article.Link}. Exception {ex}");

        //                    }

        //                    article.ImageLink = image;
        //                    article.Description = description;

        //                    await articleService.UpdateArticle(article);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"Could not set updated on for on {article.Link}. ERROR: {ex}");
        //            }


        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Something went wrong. Exception {ex}");
        //        }
        //    }
        }
}
