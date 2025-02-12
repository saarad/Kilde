using Kilde.Domain.Repositories;

namespace Kilde.Application.ApplicationServices
{
    public interface ISourceService
    {
        Task<IEnumerable<Models.Source>> GetSources();
        Task<IEnumerable<Models.Source>> GetSources(IEnumerable<string> sourceNames);
        Task<Models.Source> CreateSource(Models.Source source);
    }

    internal class SourceService : ISourceService
    {
        private readonly ISourceReadRepository _readRepository;
        private readonly ISourceWriteRepository _writeRepository;

        public SourceService(ISourceReadRepository readRepository, ISourceWriteRepository writeRepository)
        {
            _readRepository = readRepository;
            _writeRepository = writeRepository;
        }

        public async Task<IEnumerable<Models.Source>> GetSources()
        {
            var result = await _readRepository.GetSources();

            var mapped = result.Select(x => Map(x)).ToList();

            return mapped;
        }

        public async Task<IEnumerable<Models.Source>> GetSources(IEnumerable<string> sourceNames)
        {
            var result = await _readRepository.GetSources(sourceNames);

            var mapped = result.Select(x => Map(x)).ToList();

            return mapped;
        }

        public async Task<Models.Source> CreateSource(Models.Source source)
        {
            if(!Uri.IsWellFormedUriString(source.Link, UriKind.Absolute))
            {
                throw new ArgumentException("Provided link is not valid");
            }

            var mapped = Map(source);
            var result = await _writeRepository.CreateSource(mapped);

            var dto = Map(result);

            return dto;
        }

        private Models.Source Map(Domain.Models.Source source)
        {
            return new Models.Source
            {
                Link = source.Link,
                Name = source.Name,
            };
        }

        private Domain.Models.Source Map(Models.Source source)
        {
            return new Domain.Models.Source
            {
                Link = source.Link,
                Name = source.Name,
            };
        }
    }
}