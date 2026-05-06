using CodingAssistant.Domain.Entities;

namespace CodingAssistant.Application.Abstractions
{
    public interface IProjectGenerationRepository
    {
        Task AddAsync(ProjectGeneration generation, CancellationToken cancellationToken = default);

        Task<ProjectGeneration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProjectGeneration>> GetRecentAsync(
            int take,
            CancellationToken cancellationToken = default);
    }
}
