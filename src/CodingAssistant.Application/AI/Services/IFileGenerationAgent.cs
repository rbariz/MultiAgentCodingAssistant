using CodingAssistant.Application.AI.Dtos;

namespace CodingAssistant.Application.AI.Services
{
    public interface IFileGenerationAgent
    {
        
        Task<GeneratedFileAiResponse> GenerateFileAsync(
            string userPrompt,
            string? targetStack,
            ProjectPlanAiResponse plan,
            string filePath,
            CancellationToken cancellationToken = default);
    }
}
