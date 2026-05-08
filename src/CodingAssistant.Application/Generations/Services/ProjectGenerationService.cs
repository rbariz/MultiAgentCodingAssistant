using CodingAssistant.Application.Abstractions;
using CodingAssistant.Application.Generations.Dtos;
using CodingAssistant.Domain.Entities;
using CodingAssistant.Domain.Enums;

namespace CodingAssistant.Application.Generations.Services
{
    public sealed class ProjectGenerationService : IProjectGenerationService
    {
        public readonly IProjectGenerationRepository _repository;
        public readonly IUnitOfWork _unitOfWork;

        public ProjectGenerationService(
            IProjectGenerationRepository repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProjectGenerationDto> CreateAsync(
            CreateGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.UserPrompt))
                throw new ArgumentException("User prompt is required.", nameof(request.UserPrompt));

            var prompt = request.UserPrompt.Trim();

            var generation = new ProjectGeneration
            {
                UserPrompt = prompt,
                TargetStack = string.IsNullOrWhiteSpace(request.TargetStack)
                    ? "html-css-js"
                    : request.TargetStack.Trim(),
                Status = GenerationStatus.Queued,
                Steps =
                [
                    new GenerationStep
                {
                    AgentRole = AgentRole.Planner,
                    Name = "Plan project",
                    Description = "Understand the user request and create a project plan.",
                    Status = StepStatus.Pending,
                    Order = 1
                },
                new GenerationStep
                {
                    AgentRole = AgentRole.Architect,
                    Name = "Design architecture",
                    Description = "Define the project structure and required files.",
                    Status = StepStatus.Pending,
                    Order = 2
                },
                new GenerationStep
                {
                    AgentRole = AgentRole.Developer,
                    Name = "Generate files",
                    Description = "Generate the source code files.",
                    Status = StepStatus.Pending,
                    Order = 3
                },
                new GenerationStep
                {
                    AgentRole = AgentRole.Reviewer,
                    Name = "Review output",
                    Description = "Review and validate the generated project.",
                    Status = StepStatus.Pending,
                    Order = 4
                }
                ],
                Messages =
                [
                    new AgentMessage
                {
                    Role = AgentRole.User,
                    Content = prompt
                }
                ]
            };

            await _repository.AddAsync(generation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ToDto(generation);
        }

        public async Task<ProjectGenerationDto?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var generation = await _repository.GetByIdAsync(id, cancellationToken);

            return generation is null ? null : ToDto(generation);
        }

        public async Task<IReadOnlyList<ProjectGenerationDto>> GetRecentAsync(
            int take = 20,
            CancellationToken cancellationToken = default)
        {
            take = Math.Clamp(take, 1, 100);

            var generations = await _repository.GetRecentAsync(take, cancellationToken);

            return generations
                .Select(ToDto)
                .ToList();
        }

        public static ProjectGenerationDto ToDto(ProjectGeneration generation)
        {
            return new ProjectGenerationDto
            {
                Id = generation.Id,
                UserPrompt = generation.UserPrompt,
                ProjectName = generation.ProjectName,
                TargetStack = generation.TargetStack,
                Status = generation.Status.ToString(),
                CreatedAtUtc = generation.CreatedAtUtc,
                StartedAtUtc = generation.StartedAtUtc,
                CompletedAtUtc = generation.CompletedAtUtc,
                ErrorMessage = generation.ErrorMessage,

                Files = generation.Files
                    .OrderBy(x => x.Order)
                    .Select(x => new GeneratedFileDto
                    {
                        Id = x.Id,
                        RelativePath = x.RelativePath,
                        FileName = x.FileName,
                        Content = x.Content,
                        Language = x.Language,
                        Kind = x.Kind.ToString(),
                        Order = x.Order
                    })
                    .ToList(),

                Steps = generation.Steps
                    .OrderBy(x => x.Order)
                    .Select(x => new GenerationStepDto
                    {
                        Id = x.Id,
                        AgentRole = x.AgentRole.ToString(),
                        Name = x.Name,
                        Description = x.Description,
                        Status = x.Status.ToString(),
                        Order = x.Order
                    })
                    .ToList(),

                Messages = generation.Messages
                    .OrderBy(x => x.CreatedAtUtc)
                    .Select(x => new AgentMessageDto
                    {
                        Id = x.Id,
                        Role = x.Role.ToString(),
                        Content = x.Content,
                        CreatedAtUtc = x.CreatedAtUtc
                    })
                    .ToList()
            };
        }
    }
}
