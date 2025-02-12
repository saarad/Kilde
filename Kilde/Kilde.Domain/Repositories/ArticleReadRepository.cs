using Kilde.Domain.Models;
using Sanity.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kilde.Domain.Repositories
{
    internal interface IArticleReadRepository
    {
        Task<IEnumerable<Article>> GetArticles();
        Task<IEnumerable<Article>> GetArticlesBySources(IEnumerable<string> sources, int take = 20, bool takeAll = false);
    }

    internal class ArticleReadRepository : IArticleReadRepository
    {
        private readonly SanityDocumentSet<Article> _articleSet;

        public ArticleReadRepository(KildeReadClient sanityReadClient)
        {
            _articleSet = sanityReadClient.Context.DocumentSet<Article>();
        }

        public async Task<IEnumerable<Article>> GetArticles()
        {
            return await _articleSet.ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetArticlesBySources(IEnumerable<string> sources, int take = 20, bool takeAll = false)
        {
            sources = sources.Select(x => x.ToUpper()).ToList();
            var list = new List<Article>();
            foreach (var source in sources)
            {

                var articleQuery = _articleSet.Where(x => x.Source == source).OrderByDescending(x => x.LastUpdated);
                
                var articles = takeAll
                    ? await articleQuery.ToListAsync()
                    : await articleQuery.Take(take).ToListAsync();

                list.AddRange(articles);
            }
            return list.ToList();
        }

    }
}
