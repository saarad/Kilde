using Kilde.Domain.Models;
using Sanity.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kilde.Domain.Repositories
{
    internal interface ISourceReadRepository
    {
        Task<IEnumerable<Source>> GetSources();
        Task<IEnumerable<Source>> GetSources(IEnumerable<string> sourceNames);
    }

    internal class SourceReadRepository : ISourceReadRepository
    {
        private readonly SanityDocumentSet<Source> _sourceSet;

        public SourceReadRepository(KildeReadClient sanityReadClient)
        {
            _sourceSet = sanityReadClient.Context.DocumentSet<Source>();
        }

        public async Task<IEnumerable<Source>> GetSources()
        {
            return await _sourceSet.ToListAsync();
        }

        public async Task<IEnumerable<Source>> GetSources(IEnumerable<string> sourceNames)
        {
            sourceNames = sourceNames.Select(x => x.ToUpper()).ToList();
            var list = await _sourceSet.ToListAsync(); //todo sanity does not support contains
            return list.Where(x => sourceNames.Contains(x.Name)).ToList();
        }
    }
}
