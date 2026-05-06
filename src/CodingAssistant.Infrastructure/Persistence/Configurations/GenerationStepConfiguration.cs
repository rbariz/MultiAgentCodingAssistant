using CodingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodingAssistant.Infrastructure.Persistence.Configurations
{
    public sealed class GenerationStepConfiguration : IEntityTypeConfiguration<GenerationStep>
    {
        public void Configure(EntityTypeBuilder<GenerationStep> builder)
        {
            builder.ToTable("generation_steps");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AgentRole)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(4000);

            builder.HasIndex(x => new { x.ProjectGenerationId, x.Order });
        }
    }
}
