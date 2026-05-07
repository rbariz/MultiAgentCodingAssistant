using CodingAssistant.Application.AI.Dtos;
using CodingAssistant.Domain.Entities;

namespace CodingAssistant.Application.AI.Services
{
    public interface IProjectReviewerAgent
    {
        Task<ProjectReviewAiResponse> ReviewAsync(
            ProjectPlanAiResponse plan,
            IReadOnlyList<GeneratedFile> files,
            CancellationToken cancellationToken = default);
    }
}
