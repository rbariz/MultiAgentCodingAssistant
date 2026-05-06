namespace CodingAssistant.Application.AI.Dtos
{
    public sealed class GeneratedProjectAiResponse
    {
        public string ProjectName { get; set; } = "generated-app";

        public List<GeneratedProjectFileAiDto> Files { get; set; } = [];
    }
}
