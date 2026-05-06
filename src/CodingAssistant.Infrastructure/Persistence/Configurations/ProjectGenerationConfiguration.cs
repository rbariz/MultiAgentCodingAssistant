using CodingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Infrastructure.Persistence.Configurations
{

    public sealed class ProjectGenerationConfiguration : IEntityTypeConfiguration<ProjectGeneration>
    {
        public void Configure(EntityTypeBuilder<ProjectGeneration> builder)
        {
            builder.ToTable("project_generations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                    .ValueGeneratedNever();

            builder.Property(x => x.UserPrompt)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(x => x.ProjectName)
                .HasMaxLength(200);

            builder.Property(x => x.TargetStack)
                .HasMaxLength(200);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(4000);

            builder.HasMany(x => x.Files)
                .WithOne(x => x.ProjectGeneration)
                .HasForeignKey(x => x.ProjectGenerationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Steps)
                .WithOne(x => x.ProjectGeneration)
                .HasForeignKey(x => x.ProjectGenerationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Messages)
                .WithOne(x => x.ProjectGeneration)
                .HasForeignKey(x => x.ProjectGenerationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
