using CodingAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodingAssistant.Infrastructure.Persistence.Configurations
{
    public sealed class AgentMessageConfiguration : IEntityTypeConfiguration<AgentMessage>
    {
        public void Configure(EntityTypeBuilder<AgentMessage> builder)
        {
            builder.ToTable("agent_messages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Role)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Content)
                .IsRequired();

            builder.HasIndex(x => new { x.ProjectGenerationId, x.CreatedAtUtc });
        }
    }
}
