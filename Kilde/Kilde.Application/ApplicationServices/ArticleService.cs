using Kilde.Application.Models;
using Kilde.Domain.Models;
using Kilde.Domain.Repositories;
using Sanity.Linq.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kilde.Application.ApplicationServices
{
    public interface IArticleService
    {
        Task<IEnumerable<Models.Article>> GetArticles();
        Task<IEnumerable<Models.Article>> GetArticlesBySources(IEnumerable<string> sources, int take = 20, bool takeAll = false);
        Task<IEnumerable<Models.Article>> GetArticlesBySource(string source, int take = 20, bool takeAll = false);
        Task<Models.Article> CreateArticle(ArticleInputModel article, string source);
        Task<Models.Article> UpdateArticle(Models.Article article);
    }

    internal class ArticleService : IArticleService
    {
        private readonly IArticleReadRepository _readRepository;
        private readonly ISourceService _sourceApplicationService;
        private readonly IArticleWriteRepository _writeRepository;

        public ArticleService(IArticleReadRepository readRepository, ISourceService sourceApplicationService, IArticleWriteRepository writeRepository)
        {
            _readRepository = readRepository;
            _sourceApplicationService = sourceApplicationService;
            _writeRepository = writeRepository;
        }

        public async Task<IEnumerable<Models.Article>> GetArticles()
        {
            var result = await _readRepository.GetArticles();

            var mapped = result.Select(x => Map(x)).ToList();

            return mapped;
        }

        public async Task<IEnumerable<Models.Article>> GetArticlesBySources(IEnumerable<string> sources, int take = 20, bool takeAll = false)
        {
            var result = await _readRepository.GetArticlesBySources(sources, take, takeAll);

            var mapped = result.Select(x => Map(x)).ToList();

            return mapped;
        }

        public async Task<IEnumerable<Models.Article>> GetArticlesBySource(string source, int take = 20, bool takeAll = false)
        {
            var list = new List<string> { source };
            var result = await _readRepository.GetArticlesBySources(list, take, takeAll);

            var mapped = result.Select(x => Map(x)).ToList();

            return mapped;
        }

        public async Task<Models.Article> CreateArticle(ArticleInputModel article, string inputSource)
        {
            if (string.IsNullOrWhiteSpace(inputSource))
                throw new ArgumentException("Source must be specified when creating an article");

            var source = (await _sourceApplicationService.GetSources(new[] {inputSource})).FirstOrDefault();

            if(source == default)
                throw new ArgumentException("Source that is specified must exist when creating an article.");

            if (string.IsNullOrWhiteSpace(article.Title))
                throw new ArgumentException("Title must be specified when creating an article");

            if (string.IsNullOrWhiteSpace(article.Description))
                throw new ArgumentException("Description must be specified when creating an article");

            if (string.IsNullOrWhiteSpace(article.Link) || !Uri.IsWellFormedUriString(article.Link, UriKind.Absolute))
                throw new ArgumentException("Link is invalid");
            
            if(!string.IsNullOrWhiteSpace(article.ImageLink) && !Uri.IsWellFormedUriString(article.ImageLink, UriKind.Absolute))
                throw new ArgumentException("Image link is invalid");

            if(string.IsNullOrEmpty(article.Identifier))
                article.Identifier = Guid.NewGuid().ToString();

            var mapped = Map(article, inputSource);

            var result = await _writeRepository.CreateArticle(mapped);

            var dto = Map(result);

            return dto;

        }

        public async Task<Models.Article> UpdateArticle(Models.Article article)
        {
            if (string.IsNullOrWhiteSpace(article.Title))
                throw new ArgumentException("Title must be specified when creating an article");

            if (string.IsNullOrWhiteSpace(article.Description))
                throw new ArgumentException("Description must be specified when creating an article");

            if (string.IsNullOrWhiteSpace(article.Link) || !Uri.IsWellFormedUriString(article.Link, UriKind.Absolute))
                throw new ArgumentException("Link is invalid");

            if (!string.IsNullOrWhiteSpace(article.ImageLink) && !Uri.IsWellFormedUriString(article.ImageLink, UriKind.Absolute))
                throw new ArgumentException("Image link is invalid");

            var result = await _writeRepository.UpdateArticle(article.Identifier, article.Title, article.Description, article.ImageLink, article.Link, article.LastUpdated);
            var mapped = Map(result);
            
            return mapped;
        }

        private Models.Article Map(Domain.Models.Article article)
        {
            return new Models.Article
            {
                Description = article.Description,
                ImageLink = article.ImageLink,
                Source = article.Source,
                LastUpdated = article.LastUpdated,
                Link = article.Link,
                Identifier = article.Identifier,
                Title = article.Title,
            };
        }

        private Domain.Models.Article Map(ArticleInputModel article, string source)
        {
            return new Domain.Models.Article
            {
                Description = article.Description,
                ImageLink = article.ImageLink,
                Source = source,
                Link = article.Link,
                Title = article.Title,
                Identifier = article.Identifier,
                LastUpdated = article.LastUpdated,
            };
        }
    }
}
