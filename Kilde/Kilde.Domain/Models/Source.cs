using Newtonsoft.Json;

namespace Kilde.Domain.Models
{
    internal class Source
    {

        [JsonProperty("_id")]
        public string SourceId { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("_type")]
        public string DocumentType => nameof(Source).ToLower();

        public string Name { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
    }
}
