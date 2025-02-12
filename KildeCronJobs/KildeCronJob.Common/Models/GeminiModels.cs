using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KildeCronJobs.Common.Models
{

    public class GeminiPart
    {
        public string Text { get; set; } = string.Empty;
    }

    public class GeminiContent
    {
        public List<GeminiPart> Parts { get; set; } = new List<GeminiPart>();
    }

    public class GeminiRequest
    {
        public List<GeminiContent> Contents { get; set; } = new List<GeminiContent>();
        public List<GeminiSafetySettings> SafetySettings { get; set; } = new List<GeminiSafetySettings>
        {
            new GeminiSafetySettings
            {
                Category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                Threshold = "BLOCK_NONE"
            },
            new GeminiSafetySettings
            {
                Category = "HARM_CATEGORY_HATE_SPEECH",
                Threshold = "BLOCK_NONE"
            },
            new GeminiSafetySettings
            {
                Category = "HARM_CATEGORY_HARASSMENT",
                Threshold = "BLOCK_NONE"
            },
             new GeminiSafetySettings
            {
                Category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                Threshold = "BLOCK_NONE"
            }
        };
    }

    public class GeminiSafetySettings
    {
        public string Category { get; set; } = string.Empty;
        public string Threshold { get; set; } = string.Empty;
    }

    public class GeminiCandidates
    {
        public GeminiContent Content { get; set; } = null!;
    }

    public class GeminiResponse
    {
        public List<GeminiCandidates> Candidates { get; set; } = new List<GeminiCandidates>();
    }
}
