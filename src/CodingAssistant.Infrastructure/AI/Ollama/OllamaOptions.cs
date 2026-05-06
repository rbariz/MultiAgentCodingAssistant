using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Infrastructure.AI.Ollama
{
    public sealed class OllamaOptions
    {
        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string Model { get; set; } = "qwen2.5-coder:7b";
    }
}
