namespace CodingAssistant.Contracts.Generations
{
    public sealed class ProjectGenerationDto
    {
        public Guid Id { get; set; }
        public string UserPrompt { get; set; } = string.Empty;
        public string? ProjectName { get; set; }
        public string? TargetStack { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public List<GeneratedFileDto> Files { get; set; } = [];
        public List<GenerationStepDto> Steps { get; set; } = [];
    }

}
