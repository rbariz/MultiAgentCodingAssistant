namespace CodingAssistant.Contracts.Generations
{
    public sealed record LiveStepItem(string Name, string Status)
    {
        public string Name { get; set; } = Name;
        public string Status { get; set; } = Status;
    }

}
