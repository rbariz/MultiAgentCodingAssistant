using CodingAssistant.Domain.Enums;

namespace CodingAssistant.Domain.Entities
{
    public sealed class AgentMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProjectGenerationId { get; set; }

        public ProjectGeneration ProjectGeneration { get; set; } = default!;

        public AgentRole Role { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
