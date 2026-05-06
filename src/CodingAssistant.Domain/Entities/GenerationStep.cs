using CodingAssistant.Domain.Enums;

namespace CodingAssistant.Domain.Entities
{
    public sealed class GenerationStep
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProjectGenerationId { get; set; }

        public ProjectGeneration ProjectGeneration { get; set; } = default!;

        public AgentRole AgentRole { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public StepStatus Status { get; set; } = StepStatus.Pending;

        public int Order { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? StartedAtUtc { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
