using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Application.AI.Dtos
{
    public sealed class LlmChatMessage
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
    }
}
