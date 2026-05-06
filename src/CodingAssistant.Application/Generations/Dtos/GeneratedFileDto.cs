namespace CodingAssistant.Application.Generations.Dtos
{
    public sealed class GeneratedFileDto
    {
        public Guid Id { get; set; }

        public string RelativePath { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string? Language { get; set; }

        public string Kind { get; set; } = string.Empty;

        public int Order { get; set; }
    }


}
