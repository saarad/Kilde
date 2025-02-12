using HtmlAgilityPack;
using Kilde.Application.ApplicationServices;
using Kilde.Application.Models;
using KildeCronJobs.Common.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KildeCronJobs.Common.Services
{
    public class E24Service
    {
        private readonly ILogger<dynamic> _logger;
        private readonly IArticleService _articleService;
        private const string ApiKey = "";

        public E24Service(ILogger<dynamic> logger, IArticleService articleService)
        {
            _logger = logger;
            _articleService = articleService;
        }

        public async Task DoWork()
        {
            _logger.LogInformation("Starting work on E24");

            try
            {

                _logger.LogInformation($"Cron Job E24 executing at: {DateTime.Now}");

                var existingArticles = await _articleService.GetArticlesBySource("E24", takeAll: true);
                var lookup = existingArticles.ToDictionary(x => x.Identifier);

                var linksToAdd = new List<Article>();
                var linksToUpdate = new List<Article>();

                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync("https://e24.no");
                var result = await response.Content.ReadAsStringAsync();

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(result);

                var main = htmlDocument.DocumentNode.SelectSingleNode("//main");
                var gridWrapper = main.ChildNodes.Where(x => x.HasClass("grid-wrapper")).FirstOrDefault(); //ignore ads


                var articles = gridWrapper?.SelectNodes("//article") ?? throw new Exception("Could not find articles");

                foreach (var element in articles)
                {
                    var kicker = string.Empty;

                    try
                    {
                        var linkWrapper = element.ChildNodes.Where(x => x.Name == "a")
                            .FirstOrDefault() ?? throw new Exception("Could not find link for article");
                        var link = linkWrapper.GetAttributeValue("href", string.Empty);
                        if(link == string.Empty)
                        {
                            _logger.LogWarning("Link not found for article");
                            continue;
                        }

                        if (link.Contains("vgtv") || link.Contains("nyheter/a")) //ignore video and direct
                            continue;

                        //finding identifier
                        var indexOfLastSlash = link.LastIndexOf("/");
                        var linkWithoutTitle = link.Substring(0, indexOfLastSlash);
                        var indexOfIdentifier = linkWithoutTitle.LastIndexOf("/");
                        var identifier = linkWithoutTitle.Substring(indexOfIdentifier + 1);

                        var existing = existingArticles.Where(x => x.Identifier.ToLower() == identifier.ToLower()).FirstOrDefault();

                        try //search for preceding kicker in title
                        {
                            kicker = linkWrapper
                                .ChildNodes
                                .Where(x => x.HasClass("title-container"))
                                .FirstOrDefault()?
                                .ChildNodes
                                .Where(x => x.HasClass("kicker"))
                                .FirstOrDefault()?.InnerText ?? string.Empty;
                        }
                        catch
                        {
                        }

                        var imageLink = string.Empty;

                        try //search for image
                        {
                            imageLink = linkWrapper
                                .ChildNodes
                                .Where(x => x.HasClass("image-container"))
                                .FirstOrDefault()?
                                .ChildNodes
                                .Where(x => x.Name == "img")
                                .FirstOrDefault()?.GetAttributeValue("src", string.Empty) ?? string.Empty;

                        }
                        catch
                        {
                        }

                        var headline = linkWrapper
                                .ChildNodes
                                .Where(x => x.HasClass("title-container"))
                                .FirstOrDefault()?
                                .ChildNodes
                                .Where(x => x.HasClass("title"))
                                .FirstOrDefault()?.InnerText ?? string.Empty;
                        //headline = string.IsNullOrWhiteSpace(kicker) ? headline : $"{kicker} {headline}";



                        var article = new Article
                        {
                            Title = headline,
                            Identifier = identifier,
                            ImageLink = imageLink,
                            Link = link,
                            Source = "E24"
                        };

                        if (existing == null)
                        {
                            linksToAdd.Add(article);
                        }
                        else
                        {
                            linksToUpdate.Add(article);
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Could not scrape article");
                    }


                }
                var batches = linksToAdd.Chunk(55);
                foreach (var batch in batches) 
                {
                    await AddArticles(linksToAdd);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
                //await UpdateArticles(linksToUpdate, driver, _articleService);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong on top level. Error: {ex}");
                Console.WriteLine("Exception " + ex.ToString());
            }

            _logger.LogInformation("Work completed for E24.");

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

                    var headline = article.Title;
                    var description = "Klikk på linken for å lese mer";
                    var image = article.ImageLink;

                    try //attempt to get data from article, save with existing metadata on failure
                    {
                        var main = htmlLoader.DocumentNode.SelectSingleNode("//article");
                        var paragrahps = main.SelectNodes("//p");
                        var text = string.Empty;
                        paragrahps.ToList().ForEach(x => text += x.InnerText);

                        description = await geminiClient.GetGeminiSummary(text);

                    }
                    catch (Exception ex)
                    {
                       _logger.LogError(ex, $"Could not scrape description for link {article.Link}");

                    }
                    try
                    {
                        var main = htmlLoader.DocumentNode.SelectSingleNode("//article");
                        image = main.ChildNodes
                            .Where(x => x.Name == "figure")
                            .FirstOrDefault()?
                            .ChildNodes
                            .Where(x => x.Name == "img")
                            .FirstOrDefault()?
                            .GetAttributeValue("src", string.Empty) ?? image;

                        if (string.IsNullOrEmpty(image)) //if image link is empty
                        {
                            image = article.ImageLink;
                        }
                    }
                    catch { }
                    string? updatedOn = default;

                    try
                    {
                        var main = htmlLoader.GetElementbyId("main")
                            ?? throw new Exception($"Could not find main element for {article.Link}");

                        var timeTag = main.SelectSingleNode("//time[@itemprop='datePublished']")
                            .GetAttributeValue("datetime", string.Empty) ?? DateTime.Now.ToString();

                        updatedOn = DateTime.Parse(timeTag).ToUniversalTime().ToString("u").Replace(" ", "T");
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
                        LastUpdated = updatedOn ?? DateTime.Now.ToUniversalTime().ToString("u").Replace(" ", "T")
                    };

                    await _articleService.CreateArticle(mapped, "E24");
                }
                catch (Exception ex)
                {
                   _logger.LogError(ex, "Something went wrong for {article}", article.Link);
                }
            }

        }


        private async Task UpdateArticles(IEnumerable<Article> articles, IArticleService articleService)
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

            //        string? updatedOn = default;

            //        try
            //        {
            //            var main = currentDriver.FindElement(By.Id("main"));
            //            var articleTag = main.FindElement(By.TagName("article"));

            //            var timeTag = articleTag.FindElement(By.CssSelector("[itemprop='dateModified']")).GetAttribute("datetime");
            //            updatedOn = DateTime.Parse(timeTag).ToUniversalTime().ToString("u").Replace(" ", "T");

            //            if (updatedOn != article.LastUpdated)
            //            {
            //                article.LastUpdated = updatedOn;
            //                try //attempt to get new data from article, save with existing metadata on failure
            //                {
            //                    var leadText = articleTag.FindElement(By.CssSelector("[data-test-tag='lead-text']")).Text;

            //                    description = !string.IsNullOrWhiteSpace(leadText) ? leadText : description;
            //                }
            //                catch (Exception ex)
            //                {
            //                    Console.WriteLine($"Could not scrape description for link {article.Link}. Exception {ex}");

            //                }

            //                article.ImageLink = image;
            //                article.Description = description;

            //                await articleService.UpdateArticle(article);
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine($"Could not set updated on for on {article.Link}. ERROR: {ex}");
            //        }


            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Something went wrong. Exception {ex}");
            //    }
            //}
        }
    }
}
