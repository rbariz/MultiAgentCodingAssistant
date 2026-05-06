using CodingAssistant.Domain.Enums;

namespace CodingAssistant.Domain.Entities
{
    public sealed class GeneratedFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProjectGenerationId { get; set; }

        public ProjectGeneration ProjectGeneration { get; set; } = default!;

        public string RelativePath { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string? Language { get; set; }

        public GeneratedFileKind Kind { get; set; } = GeneratedFileKind.SourceCode;

        public int Order { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
