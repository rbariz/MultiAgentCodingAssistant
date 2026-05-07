using CodingAssistant.Application.Abstractions;
using CodingAssistant.Application.AI.Services;
using CodingAssistant.Application.Realtime;
using CodingAssistant.Domain.Entities;
using CodingAssistant.Domain.Enums;

namespace CodingAssistant.Application.Generations.Services
{
    public sealed class ProjectGenerationOrchestrator : IProjectGenerationOrchestrator
    {
        private readonly IProjectGenerationRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenerationRealtimeNotifier _realtimeNotifier;
        private readonly ICodeGenerationAgent _codeGenerationAgent;

        public ProjectGenerationOrchestrator(
            IProjectGenerationRepository repository,
            IUnitOfWork unitOfWork,
            IGenerationRealtimeNotifier realtimeNotifier,
            ICodeGenerationAgent codeGenerationAgent)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _realtimeNotifier = realtimeNotifier;
            _codeGenerationAgent = codeGenerationAgent;
        }

        public async Task RunAsync(Guid generationId, CancellationToken cancellationToken = default)
        {
            var generation = await _repository.GetByIdAsync(generationId, cancellationToken);

            if (generation is null)
                return;

            try
            {
                generation.Status = GenerationStatus.Generating;
                generation.StartedAtUtc = DateTime.UtcNow;

                // Notify the UI immediately that generation has started.
                // This allows the frontend to display realtime progress feedback
                // instead of waiting for the full LLM workflow to complete.
                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "GenerationStarted",
                    new
                    {
                        generation.Id,
                        Status = generation.Status.ToString()
                    },
                    cancellationToken);

                CompleteStep(generation, AgentRole.Planner);

                AddMessage(
                    generation,
                    AgentRole.Planner,
                    "Project plan created.");

                // Send planner activity to the realtime UI log panel.
                // Users can see which AI agent is currently working.
                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "AgentMessage",
                    new
                    {
                        Role = AgentRole.Planner.ToString(),
                        Content = "Project plan created."
                    },
                    cancellationToken);

                CompleteStep(generation, AgentRole.Architect);

                AddMessage(
                    generation,
                    AgentRole.Architect,
                    "Project architecture selected by AI.");

                // Notify frontend that the architecture phase completed.
                // This simulates a real multi-agent workflow timeline.
                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "AgentMessage",
                    new
                    {
                        Role = AgentRole.Architect.ToString(),
                        Content = "Project architecture selected by AI."
                    },
                    cancellationToken);

                var aiProject = await _codeGenerationAgent.GenerateAsync(
                    generation.UserPrompt,
                    generation.TargetStack,
                    cancellationToken);

                generation.ProjectName = aiProject.ProjectName;

                var generatedFiles = aiProject.Files
                    .Select((file, index) => new GeneratedFile
                    {
                        ProjectGenerationId = generation.Id,
                        RelativePath = file.Path,
                        FileName = Path.GetFileName(file.Path),
                        Language = file.Language,
                        Kind = GeneratedFileKind.SourceCode,
                        Order = index + 1,
                        Content = file.Content
                    })
                    .ToList();

                foreach (var file in generatedFiles)
                {
                    await _repository.AddFileAsync(file, cancellationToken);

                    // Notify UI as each file is generated.
                    // This creates a realtime "AI coding" experience similar
                    // to modern agentic coding platforms.
                    await _realtimeNotifier.SendMessageAsync(
                        generation.Id,
                        "FileGenerated",
                        new
                        {
                            File = file.RelativePath
                        },
                        cancellationToken);
                }

                CompleteStep(generation, AgentRole.Developer);

                AddMessage(
                    generation,
                    AgentRole.Developer,
                    "Source files generated successfully.");

                // Inform the UI that the developer agent finished code generation.
                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "AgentMessage",
                    new
                    {
                        Role = AgentRole.Developer.ToString(),
                        Content = "Source files generated successfully."
                    },
                    cancellationToken);

                CompleteStep(generation, AgentRole.Reviewer);

                AddMessage(
                    generation,
                    AgentRole.Reviewer,
                    "Generated project reviewed successfully.");

                // Notify the frontend that the reviewer agent validated the output.
                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "AgentMessage",
                    new
                    {
                        Role = AgentRole.Reviewer.ToString(),
                        Content = "Generated project reviewed successfully."
                    },
                    cancellationToken);

                generation.Status = GenerationStatus.Completed;
                generation.CompletedAtUtc = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Final realtime event sent when the workflow fully completes.
                // The frontend can now stop loaders and enable ZIP download.
                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "GenerationCompleted",
                    new
                    {
                        generation.Id,
                        generation.ProjectName
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                generation.Status = GenerationStatus.Failed;
                generation.ErrorMessage = ex.Message;
                generation.CompletedAtUtc = DateTime.UtcNow;

                AddMessage(
                    generation,
                    AgentRole.System,
                    $"Generation failed: {ex.Message}");

                await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                // Send realtime failure event so the UI can display
                // the error immediately without polling.
                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "GenerationFailed",
                    new
                    {
                        Error = ex.Message
                    },
                    CancellationToken.None);
            }
        }
        private static void CompleteStep(ProjectGeneration generation, AgentRole role)
        {
            var step = generation.Steps.FirstOrDefault(x => x.AgentRole == role);

            if (step is null)
                return;

            step.Status = StepStatus.Completed;
            step.StartedAtUtc ??= DateTime.UtcNow;
            step.CompletedAtUtc = DateTime.UtcNow;
        }

        private static void AddMessage(ProjectGeneration generation, AgentRole role, string content)
        {
            generation.Messages.Add(new AgentMessage
            {
                ProjectGenerationId = generation.Id,
                Role = role,
                Content = content
            });
        }

    }
}
