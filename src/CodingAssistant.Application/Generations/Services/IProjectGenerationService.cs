using CodingAssistant.Application.Generations.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Application.Generations.Services
{
    public interface IProjectGenerationService
    {
        Task<ProjectGenerationDto> CreateAsync(
            CreateGenerationRequest request,
            CancellationToken cancellationToken = default);

        Task<ProjectGenerationDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ProjectGenerationDto>> GetRecentAsync(
            int take = 20,
            CancellationToken cancellationToken = default);
    }
}
