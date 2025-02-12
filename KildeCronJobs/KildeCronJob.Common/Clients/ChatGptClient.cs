using KildeCronJobs.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace KildeCronJobs.Common.Clients
{
    public class ChatGptClient
    {
        private readonly string _token = "";
        public ChatGptClient(string token)
        {
            _token = token;
        }

        public async Task<string> GetChatGptSummary(string text)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(3);

            var content = $"Basert på denne artikkelteksten, gi en spennende oppsummering på maks 50 ord på norsk: {text}";
            var message = new Message { Content = content, Role = "user" };
            var request = new ChatGptRequest { Model = "gpt-3.5-turbo-1106", Messages = new Message[] { message }, Temperature = 0.7 };
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
            var chatGptResponse = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", request);
            var chatGptResult = await chatGptResponse.Content.ReadAsStringAsync();
            var completion = JsonConvert.DeserializeObject<ChatGptResponse>(chatGptResult);
            var summary = completion?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

            if (string.IsNullOrEmpty(summary))
            {
                Console.WriteLine("WARNING: summary was empty");
            }

            return summary;
        }

    }
}
