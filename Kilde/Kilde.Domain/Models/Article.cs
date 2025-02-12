using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kilde.Domain.Models
{
    internal class Article
    {
        [JsonProperty("_id")]
        public string ArticleId { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("_type")]
        public string DocumentType => nameof(Article).ToLower();

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string LastUpdated { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string ImageLink { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
    }
}
