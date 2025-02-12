using Kilde.Domain.Models;
using Sanity.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kilde.Domain.Repositories
{
    internal interface ISourceWriteRepository
    {
        Task<Source> CreateSource(Source source);
    }

    internal class SourceWriteRepository : ISourceWriteRepository
    {
        private readonly SanityDocumentSet<Source> _sourceSet;

        public SourceWriteRepository(KildeWriteClient client)
        {
            _sourceSet = client.Context.DocumentSet<Source>();
        }

        public async Task<Source> CreateSource(Source source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            source.Name = source.Name.ToUpper();
            var exists = await _sourceSet.Where(x => x.Name == source.Name).FirstOrDefaultAsync();
            if (exists != null)
            {
                return exists;
            }

            var result = await _sourceSet.Create(source).CommitAsync();
            if (result.Results.Any())
                return result.Results.First().Document;
            else
                throw new Exception("Result yielded empty, no rows added");
        }
    }
}
