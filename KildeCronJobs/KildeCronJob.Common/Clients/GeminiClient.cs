using KildeCronJobs.Common.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace KildeCronJobs.Common.Clients
{
    public class GeminiClient
    {
        private readonly string _apiKey;

        public GeminiClient(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> GetGeminiSummary(string articleText)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(3);
            var request = CreateGeminiRequest(articleText);
            var response = await httpClient.PostAsJsonAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}", request);
            var result = await response.Content.ReadAsStringAsync();
            var geminiContent = JsonConvert.DeserializeObject<GeminiResponse>(result);

            var summary = geminiContent?.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text ?? string.Empty;

            return summary;
        }

        private GeminiRequest CreateGeminiRequest(string articleText)
        {
            var content = $"Basert på denne nyhetsartikkelen, gi en spennende sammendrag på maks 60 ord og ikke mindre enn 40 ord på norsk. " +
                $"Sammendraget skal inneholde hovedpoengene i artikkelen og hva den konkluderer med: {articleText}";
            var message = new GeminiContent { Parts = new List<GeminiPart> { new GeminiPart { Text = content } } };
            var request = new GeminiRequest { Contents = new List<GeminiContent> { message } };

            return request;
        }

    }
}
