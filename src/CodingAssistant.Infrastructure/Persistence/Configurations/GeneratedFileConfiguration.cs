using CodingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodingAssistant.Infrastructure.Persistence.Configurations
{
    public sealed class GeneratedFileConfiguration : IEntityTypeConfiguration<GeneratedFile>
    {
        public void Configure(EntityTypeBuilder<GeneratedFile> builder)
        {
            builder.ToTable("generated_files");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RelativePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.Language)
                .HasMaxLength(100);

            builder.Property(x => x.Kind)
                .HasConversion<int>()
                .IsRequired();

            builder.HasIndex(x => new { x.ProjectGenerationId, x.RelativePath })
                .IsUnique();
        }
    }
}
