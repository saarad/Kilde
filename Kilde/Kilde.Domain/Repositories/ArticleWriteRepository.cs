using Kilde.Domain.Models;
using Sanity.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kilde.Domain.Repositories
{
    internal interface IArticleWriteRepository
    {
        Task<Article> CreateArticle(Article article);
        Task<Article> UpdateArticle(string identifier,
                                                 string title = "",
                                                 string description = "",
                                                 string imageLink = "",
                                                 string link = "",
                                                 string lastUpdatedOn = "");
    }
    internal class ArticleWriteRepository : IArticleWriteRepository
    {
        private readonly SanityDocumentSet<Article> _articleSet;

        public ArticleWriteRepository(KildeWriteClient client)
        {
            _articleSet = client.Context.DocumentSet<Article>();
        }

        public async Task<Article> CreateArticle(Article article)
        {
            ArgumentNullException.ThrowIfNull(article, nameof(article));
            article.Identifier = article.Identifier.ToLower();

            if(article.LastUpdated != default && !string.IsNullOrWhiteSpace(article.LastUpdated))
            {
                article.LastUpdated = DateTime.Parse(article.LastUpdated)
                    .ToUniversalTime()
                    .ToString("u")
                    .Replace(" ", "T");
            }
            else
            {
                article.LastUpdated = DateTime.Now.ToUniversalTime().ToString("u").Replace(" ", "T");
            }


            var exists = await _articleSet.Where(x => x.Identifier == article.Identifier).FirstOrDefaultAsync();

            if (exists != null)
            {
                return exists;
            }

            var result = await _articleSet.Create(article).CommitAsync();
            if (result.Results.Any())
                return result.Results.First().Document;
            else
                throw new Exception("Result yielded empty, no rows added");
        }

        public async Task<Article> UpdateArticle(string identifier,
                                                 string title = "",
                                                 string description = "",
                                                 string imageLink = "",
                                                 string link = "",
                                                 string lastUpdatedOn = "")
        {
            var existing = await _articleSet.Where(x => x.Identifier == identifier).FirstOrDefaultAsync();
            if(existing == null)
            {
                throw new ArgumentException($"No article with identifier {identifier} is found"); 
            }

            if(!string.IsNullOrEmpty(title))
            {
                existing.Title = title;
            }
            if(!string.IsNullOrEmpty(description))
            {
                existing.Description = description;
            }
            if(!string.IsNullOrEmpty(imageLink))
            {
                existing.ImageLink = imageLink;
            }
            if(!string.IsNullOrEmpty(link))
            {
                existing.Link = link;
            }
            if (!string.IsNullOrEmpty(lastUpdatedOn))
            {
                existing.LastUpdated = lastUpdatedOn;
            }

            await _articleSet.Update(existing).CommitAsync();

            return existing;
        }
    }
}
