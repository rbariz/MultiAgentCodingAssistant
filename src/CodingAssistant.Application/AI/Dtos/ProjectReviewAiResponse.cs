namespace CodingAssistant.Application.AI.Dtos
{
    public sealed class ProjectReviewAiResponse
    {
        public bool IsValid { get; set; }

        public string Summary { get; set; } = string.Empty;

        public List<string> Issues { get; set; } = [];

        public List<string> Suggestions { get; set; } = [];
    }
}
