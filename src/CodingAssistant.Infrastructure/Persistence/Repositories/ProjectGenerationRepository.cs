using CodingAssistant.Application.Abstractions;
using CodingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Infrastructure.Persistence.Repositories
{
    

    public sealed class ProjectGenerationRepository : IProjectGenerationRepository
    {
        private readonly CodingAssistantDbContext _db;

        public ProjectGenerationRepository(CodingAssistantDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(
            ProjectGeneration generation,
            CancellationToken cancellationToken = default)
        {
            await _db.ProjectGenerations.AddAsync(generation, cancellationToken);
        }

        public async Task<ProjectGeneration?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _db.ProjectGenerations
                .Include(x => x.Files)
                .Include(x => x.Steps)
                .Include(x => x.Messages)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<ProjectGeneration>> GetRecentAsync(
            int take,
            CancellationToken cancellationToken = default)
        {
            return await _db.ProjectGenerations
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task AddFileAsync(
    GeneratedFile file,
    CancellationToken cancellationToken = default)
        {
            await _db.GeneratedFiles.AddAsync(file, cancellationToken);
        }

    }
}
