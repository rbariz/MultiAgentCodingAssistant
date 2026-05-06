using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Application.Generations.Dtos
{
    public sealed class CreateGenerationRequest
    {
        public string UserPrompt { get; set; } = string.Empty;

        public string? TargetStack { get; set; }
    }


}
