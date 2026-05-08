namespace CodingAssistant.Contracts.Generations
{
    public sealed class GenerationStepDto
    {
        public string AgentRole { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Order { get; set; }
    }

}
