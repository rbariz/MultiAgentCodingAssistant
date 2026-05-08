namespace CodingAssistant.Contracts.Generations
{
    public sealed class FileDeltaEvent
    {
        public string File { get; set; } = string.Empty;
        public string Delta { get; set; } = string.Empty;
    }

}
