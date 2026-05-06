namespace CodingAssistant.Application.Generations.Services
{
    public interface IProjectGenerationOrchestrator
    {
        Task RunAsync(Guid generationId, CancellationToken cancellationToken = default);
    }
}
