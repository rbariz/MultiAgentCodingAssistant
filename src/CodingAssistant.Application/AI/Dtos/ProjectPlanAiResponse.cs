namespace CodingAssistant.Application.AI.Dtos
{
    public sealed class ProjectPlanAiResponse
    {
        public string ProjectName { get; set; } = "generated-app";
        public string ProjectType { get; set; } = "web-app";
        public string Description { get; set; } = string.Empty;
        public List<string> Files { get; set; } = [];
        public List<string> ArchitectureNotes { get; set; } = [];
    }
}
