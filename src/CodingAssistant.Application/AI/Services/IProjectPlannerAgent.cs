using CodingAssistant.Application.AI.Dtos;

namespace CodingAssistant.Application.AI.Services
{
    public interface IProjectPlannerAgent
    {
        Task<ProjectPlanAiResponse> CreatePlanAsync(
            string userPrompt,
            string? targetStack,
            CancellationToken cancellationToken = default);
    }
}
