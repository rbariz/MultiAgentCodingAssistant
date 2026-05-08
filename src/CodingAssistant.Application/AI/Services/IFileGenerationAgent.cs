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

        IAsyncEnumerable<string> StreamFilePreviewAsync(
    string userPrompt,
    string? targetStack,
    ProjectPlanAiResponse plan,
    string filePath,
    CancellationToken cancellationToken = default);

        IAsyncEnumerable<string> StreamFileContentAsync(
    string userPrompt,
    string? targetStack,
    ProjectPlanAiResponse plan,
    string filePath,
    CancellationToken cancellationToken = default);
    }


}
