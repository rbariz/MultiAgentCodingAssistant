namespace CodingAssistant.Contracts.Generations
{
    public sealed record LiveFileItem(string Path, string Status)
    {
        public string Path { get; set; } = Path;
        public string Status { get; set; } = Status;
    }

}
