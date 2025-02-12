namespace KildeCronJobs.Common.Models
{
    using System.Collections.Generic;

    public class Message
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class ChatGptRequest
    {
        public string Model { get; set; } = string.Empty;
        public IEnumerable<Message> Messages { get; set; } = new List<Message>();
        public double Temperature { get; set; }
    }

    public class ChatGptChoice
    {
        public int Index { get; set; }
        public Message Message { get; set; } = new();
        public string FinishReason { get; set; } = string.Empty;
    }

    public class ChatGptUsage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    public class ChatGptResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Object { get; set; } = string.Empty;
        public int Created { get; set; }
        public string Model { get; set; } = string.Empty;
        public List<ChatGptChoice> Choices { get; set; } = new();
        public ChatGptUsage Usage { get; set; } = new();
    }
}