namespace CodingAssistant.Application.Generations.Dtos
{
    public sealed class AgentMessageDto
    {
        public Guid Id { get; set; }

        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }
    }


}
