namespace CodingAssistant.Application.Generations.Dtos
{
    public sealed class ProjectGenerationDto
    {
        public Guid Id { get; set; }

        public string UserPrompt { get; set; } = string.Empty;

        public string? ProjectName { get; set; }

        public string? TargetStack { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? StartedAtUtc { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

        public string? ErrorMessage { get; set; }

        public List<GeneratedFileDto> Files { get; set; } = [];

        public List<GenerationStepDto> Steps { get; set; } = [];

        public List<AgentMessageDto> Messages { get; set; } = [];
    }


}
