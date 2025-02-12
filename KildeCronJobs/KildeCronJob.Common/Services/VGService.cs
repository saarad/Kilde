using HtmlAgilityPack;
using Kilde.Application.ApplicationServices;
using Kilde.Application.Models;
using KildeCronJobs.Common.Clients;
using KildeCronJobs.Common.Domain;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KildeCronJobs.Common.Services
{
    public class VGService
    {
        private readonly ILogger<dynamic> _logger;
        private readonly IArticleService _articleService;
        private const string ApiKey = "";
        public VGService(ILogger<dynamic> logger, IArticleService articleService)
        {
            _logger = logger;
            _articleService = articleService;
        }

        public async Task DoWork()
        {
            try
            {
                _logger.LogInformation("Cron Job VG started execution at: {DateTime.Now}", DateTime.Now.ToString());

                var existingArticles = await _articleService.GetArticlesBySource("VG", takeAll: true);
                var lookup = existingArticles.ToDictionary(x => x.Identifier);

                var linksToAdd = new List<VGArticleData>();
                var linksToUpdate = new List<Article>();

                using var httpClient = new HttpClient();

                var response = await httpClient.GetAsync("https://vg.no");
                var result = await response.Content.ReadAsStringAsync();

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(result);

                var main = htmlDocument.DocumentNode.SelectSingleNode("//main");
                var articles = main.SelectNodes("//article");

                foreach (var element in articles)
                {

                    var article = element.ChildNodes.Where(x => x.Name == "div").FirstOrDefault();

                    if (article == null)
                    {
                        _logger.LogWarning("Div element was null for article, skipping scrape");
                        continue;
                    }

                    var linkWrapper = article.ChildNodes.Where(x => x.Name == "a").FirstOrDefault();

                    if (linkWrapper == null)
                    {
                        _logger.LogWarning("Link element was null for article, skipping scrape");
                        continue;
                    }

                    var link = linkWrapper.GetAttributeValue("href", "");

                    var headline = linkWrapper.ChildNodes
                        .Where(x => x.Name == "div")
                        .FirstOrDefault()?
                        .ChildNodes?
                        .Where(x => x.HasClass("headline"))?
                        .FirstOrDefault()?
                        .InnerText ?? string.Empty;

                    if (string.IsNullOrEmpty(link))
                        continue;

                    try
                    {
                        var script = element.ChildNodes.Where(x => x.Name == "script").FirstOrDefault();  //vg tracking data

                        if (script == null) //no use scraping data without metadata, usually games etc.
                        {
                            continue;
                        }

                        var value = script.InnerHtml;
                        var vgArticleData = JsonConvert.DeserializeObject<VGArticleData>(value) ?? new();

                        vgArticleData.Link = link;
                        vgArticleData.HeadLine = headline;

                        //todo: fetch spesial
                        if (vgArticleData.Brand != "vg.no" || link.Contains("spesial") && vgArticleData.Changes == null) //exclude tv, direct etc
                            continue;

                        if (!lookup.ContainsKey(vgArticleData.ArticleId.ToLower()))
                        {
                            linksToAdd.Add(vgArticleData);
                        }
                        else
                        {
                            var existing = lookup.GetValueOrDefault(vgArticleData.ArticleId.ToLower());
                            var updatedOn = vgArticleData.Changes?.Updated != null
                                ? vgArticleData.Changes.Updated.ToUniversalTime().ToString("u").Replace(" ", "T")
                                : existing!.LastUpdated;

                            if (existing != null && existing.LastUpdated != updatedOn)
                            {
                                existing.LastUpdated = updatedOn;
                                existing.Title = vgArticleData.HeadLine;
                                existing.Link = vgArticleData.Link;

                                linksToUpdate.Add(existing);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not scrape article with title {headline}");
                        Console.WriteLine(ex.ToString());
                        _logger.LogError(ex, $"Could not scrape article with title {headline}");
                    }

                }
                var batches = linksToAdd.Chunk(55); //to avoid triggering paid gemini subscription
                foreach (var batch in batches)
                {
                    await AddArticles(batch);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
                //await UpdateArticles(linksToUpdate, driver);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong on top level. Error: {ex}");
                Console.WriteLine("Exception " + ex.ToString());
            }

            _logger.LogInformation($"Work completed for VG at: {DateTime.Now}.");
        }

        private async Task AddArticles(IEnumerable<VGArticleData> articles)
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

                    var headline = article.HeadLine;
                    var description = "Klikk på linken for å lese mer";
                    var image = string.Empty;

                    try //attempt to get data from article, save with existing metadata on failure
                    {
                        var main = htmlLoader.DocumentNode.SelectSingleNode("//article");
                        var paragrahps = main.SelectNodes("//p");
                        var text = string.Empty;
                        paragrahps.ToList().ForEach(x => text += x.InnerText);

                        var summary = await geminiClient.GetGeminiSummary(text);

                        if (string.IsNullOrEmpty(summary))
                        {
                            Console.WriteLine("WARNING: summary was empty");
                        }
                        else
                        {
                            description = summary;
                        }

                        image = main.ChildNodes
                            .Where(x => x.Name == "figure")
                            .FirstOrDefault()?
                            .ChildNodes?
                            .Where(x => x.Name == "img")
                            .FirstOrDefault()?
                            .GetAttributeValue("src", "") ?? image;

                        headline = main.SelectSingleNode("//h1[@data-test-tag='headline']")?.InnerText ?? headline;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Could not scrape for link {article.Link}. Exception {ex}");

                    }

                    string? updatedOn = default;

                    try
                    {
                        updatedOn = article.Changes.Updated.ToUniversalTime().ToString("u").Replace(" ", "T");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not set updated on. Error: " + ex);
                    }

                    var mapped = new ArticleInputModel
                    {
                        Description = description,
                        Identifier = article.ArticleId,
                        ImageLink = image,
                        Link = article.Link,
                        Title = headline,
                        LastUpdated = updatedOn ?? DateTime.Now.ToUniversalTime().ToString("u").Replace(" ", "T")
                    };

                    await _articleService.CreateArticle(mapped, "VG");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Something went wrong. Exception {ex}");
                }
            }

        }

        private async Task UpdateArticles(IEnumerable<Article> articles)
        {
            //foreach (var article in articles)
            //{
            //    try
            //    {
            //        await Task.Delay(2000);
            //        currentDriver.Navigate().GoToUrl(article.Link);

            //        var headline = article.Title;
            //        var description = article.Description;
            //        var image = article.ImageLink;

            //        try //attempt to get data from article, save with existing metadata on failure
            //        {
            //            var main = currentDriver.FindElement(By.Id("main"));
            //            var articleTag = main.FindElement(By.TagName("article"));


            //            var leadText = articleTag.FindElement(By.CssSelector("[data-test-tag='lead-text']")).Text;

            //            description = !string.IsNullOrWhiteSpace(leadText) ? leadText : description;

            //            image = articleTag.FindElement(By.TagName("img")).GetAttribute("src");
            //            headline = articleTag.FindElement(By.TagName("h1")).Text;
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine($"Could not scrape for link {article.Link}. Exception {ex}");

            //        }

            //        article.ImageLink = image;
            //        article.Description = description;

            //        await _articleService.UpdateArticle(article);
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Something went wrong. Exception {ex}");
            //    }
            //}
        }
    }
}
