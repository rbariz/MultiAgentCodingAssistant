using CodingAssistant.Application.AI.Dtos;

namespace CodingAssistant.Application.AI.Services
{
    public interface ICodeGenerationAgent
    {
        Task<GeneratedProjectAiResponse> GenerateAsync(
            string userPrompt,
            string? targetStack,
            CancellationToken cancellationToken = default);
    }
}
