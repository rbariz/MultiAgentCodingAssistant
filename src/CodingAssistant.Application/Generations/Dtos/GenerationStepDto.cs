namespace CodingAssistant.Application.Generations.Dtos
{
    public sealed class GenerationStepDto
    {
        public Guid Id { get; set; }

        public string AgentRole { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Status { get; set; } = string.Empty;

        public int Order { get; set; }
    }


}
