using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kilde.Application.Models
{
    public class ArticleInputModel
    {
        [JsonProperty(Required = Required.Always)]
        public string Title { get; set; } = string.Empty;
        [JsonProperty(Required = Required.Always)]
        public string Description { get; set; } = string.Empty;
        [JsonProperty(Required = Required.Default)]
        public string ImageLink { get; set; } = string.Empty;
        [JsonProperty(Required = Required.Always)]
        public string Link { get; set; } = string.Empty;
        [JsonProperty(Required = Required.Default)]
        public string Identifier { get; set; } = string.Empty;
        [JsonProperty(Required = Required.Default)]
        public string LastUpdated { get; set; } = string.Empty;
    }
}
