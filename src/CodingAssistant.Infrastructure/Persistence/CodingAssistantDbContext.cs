using CodingAssistant.Application.Abstractions;
using CodingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Infrastructure.Persistence
{
    
    public sealed class CodingAssistantDbContext : DbContext
    {
        public CodingAssistantDbContext(DbContextOptions<CodingAssistantDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProjectGeneration> ProjectGenerations => Set<ProjectGeneration>();
        public DbSet<GeneratedFile> GeneratedFiles => Set<GeneratedFile>();
        public DbSet<GenerationStep> GenerationSteps => Set<GenerationStep>();
        public DbSet<AgentMessage> AgentMessages => Set<AgentMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CodingAssistantDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
