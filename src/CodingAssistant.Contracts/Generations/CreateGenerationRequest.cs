namespace CodingAssistant.Contracts.Generations
{
    public sealed class CreateGenerationRequest
    {
        public string UserPrompt { get; set; } = string.Empty;
        public string? TargetStack { get; set; }
    }

}
