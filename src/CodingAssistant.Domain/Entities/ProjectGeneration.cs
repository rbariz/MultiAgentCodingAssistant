using CodingAssistant.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Domain.Entities
{

    public sealed class ProjectGeneration
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserPrompt { get; set; } = string.Empty;

        public string? ProjectName { get; set; }

        public string? TargetStack { get; set; }

        public GenerationStatus Status { get; set; } = GenerationStatus.Draft;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? StartedAtUtc { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

        public string? ErrorMessage { get; set; }

        public List<GeneratedFile> Files { get; set; } = [];

        public List<GenerationStep> Steps { get; set; } = [];

        public List<AgentMessage> Messages { get; set; } = [];
    }
}
