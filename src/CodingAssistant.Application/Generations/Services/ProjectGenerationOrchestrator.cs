using CodingAssistant.Application.Abstractions;
using CodingAssistant.Application.AI.Services;
using CodingAssistant.Application.Realtime;
using CodingAssistant.Domain.Entities;
using CodingAssistant.Domain.Enums;
using System.Text;

namespace CodingAssistant.Application.Generations.Services
{
    public sealed class ProjectGenerationOrchestrator : IProjectGenerationOrchestrator
    {
        public readonly IProjectGenerationRepository _repository;
        public readonly IUnitOfWork _unitOfWork;
        public readonly IGenerationRealtimeNotifier _realtimeNotifier;
        public readonly ICodeGenerationAgent _codeGenerationAgent;
        public readonly IProjectPlannerAgent _plannerAgent;
        public readonly IFileGenerationAgent _fileGenerationAgent;
        public readonly IProjectReviewerAgent _reviewerAgent;

        public ProjectGenerationOrchestrator(
            IProjectGenerationRepository repository,
            IUnitOfWork unitOfWork,
            IGenerationRealtimeNotifier realtimeNotifier,
            ICodeGenerationAgent codeGenerationAgent,
            IProjectPlannerAgent plannerAgent,
            IFileGenerationAgent fileGenerationAgent,
            IProjectReviewerAgent reviewerAgent)      
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _realtimeNotifier = realtimeNotifier;
            _codeGenerationAgent = codeGenerationAgent;
            _plannerAgent = plannerAgent;
            _fileGenerationAgent = fileGenerationAgent;
            _reviewerAgent = reviewerAgent;
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

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "GenerationStarted",
                    new { generation.Id, Status = generation.Status.ToString() },
                    cancellationToken);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "StepStarted",
                    new { Step = "Planner", Status = "Running" },
                    cancellationToken);

                var plan = await _plannerAgent.CreatePlanAsync(
                    generation.UserPrompt,
                    generation.TargetStack,
                    cancellationToken);

                generation.ProjectName = plan.ProjectName;

                CompleteStep(generation, AgentRole.Planner);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "StepCompleted",
                    new { Step = "Planner", Status = "Completed" },
                    cancellationToken);

                var plannerMessage = $"Project plan created: {plan.ProjectName}.";
                AddMessage(generation, AgentRole.Planner, plannerMessage);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "AgentMessage",
                    new { Role = AgentRole.Planner.ToString(), Content = plannerMessage },
                    cancellationToken);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "StepStarted",
                    new { Step = "Architect", Status = "Running" },
                    cancellationToken);

                var architectureSummary = string.Join(", ", plan.Files);

                CompleteStep(generation, AgentRole.Architect);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "StepCompleted",
                    new { Step = "Architect", Status = "Completed" },
                    cancellationToken);

                var architectMessage = $"Project files selected: {architectureSummary}.";
                AddMessage(generation, AgentRole.Architect, architectMessage);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "AgentMessage",
                    new { Role = AgentRole.Architect.ToString(), Content = architectMessage },
                    cancellationToken);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "StepStarted",
                    new { Step = "Developer", Status = "Running" },
                    cancellationToken);

                var generatedFiles = new List<GeneratedFile>();
                var order = 1;


                foreach (var plannedFile in plan.Files)
                {
                    var startFileMessage = $"Generating file: {plannedFile}";
                    AddMessage(generation, AgentRole.Developer, startFileMessage);

                    await _realtimeNotifier.SendMessageAsync(
                        generation.Id,
                        "FileGenerationStarted",
                        new { File = plannedFile },
                        cancellationToken);

                    await _realtimeNotifier.SendMessageAsync(
                        generation.Id,
                        "AgentMessage",
                        new { Role = AgentRole.Developer.ToString(), Content = startFileMessage },
                        cancellationToken);

                    // Notify the UI that the AI streaming preview for this file is starting.
                    // This gives the user immediate feedback while Ollama prepares the final file content.
                    await _realtimeNotifier.SendMessageAsync(
                        generation.Id,
                        "FileContentStreamingStarted",
                        new { File = plannedFile },
                        cancellationToken);

                    

                    // Notify the UI that the streaming preview is complete.
                    // After this, the backend starts the actual structured JSON file generation.
                    await _realtimeNotifier.SendMessageAsync(
                        generation.Id,
                        "FileContentStreamingCompleted",
                        new { File = plannedFile },
                        cancellationToken);

                    var contentBuilder = new StringBuilder();

                    await _realtimeNotifier.SendMessageAsync(
                        generation.Id,
                        "FileContentStreamingStarted",
                        new { File = plannedFile },
                        cancellationToken);

                    await foreach (var token in _fileGenerationAgent.StreamFileContentAsync(
                        generation.UserPrompt,
                        generation.TargetStack,
                        plan,
                        plannedFile,
                        cancellationToken))
                    {
                        contentBuilder.Append(token);

                        await _realtimeNotifier.SendMessageAsync(
                            generation.Id,
                            "FileContentDelta",
                            new
                            {
                                File = plannedFile,
                                Delta = token
                            },
                            cancellationToken);
                    }

                    await _realtimeNotifier.SendMessageAsync(
                        generation.Id,
                        "FileContentStreamingCompleted",
                        new { File = plannedFile },
                        cancellationToken);

                    var generatedFile = new GeneratedFile
                    {
                        ProjectGenerationId = generation.Id,
                        RelativePath = plannedFile,
                        FileName = Path.GetFileName(plannedFile),
                        Language = InferLanguage(plannedFile),
                        Kind = GeneratedFileKind.SourceCode,
                        Order = order++,
                        Content = CleanStreamedContent(contentBuilder.ToString())
                    };

                    await _repository.AddFileAsync(generatedFile, cancellationToken);
                    generatedFiles.Add(generatedFile);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    await _realtimeNotifier.SendMessageAsync(
                        generation.Id,
                        "FileGenerated",
                        new { File = generatedFile.RelativePath },
                        cancellationToken);
                }
                CompleteStep(generation, AgentRole.Developer);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "StepCompleted",
                    new { Step = "Developer", Status = "Completed" },
                    cancellationToken);

                var developerMessage = "Source files generated successfully.";
                AddMessage(generation, AgentRole.Developer, developerMessage);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "AgentMessage",
                    new { Role = AgentRole.Developer.ToString(), Content = developerMessage },
                    cancellationToken);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "StepStarted",
                    new { Step = "Reviewer", Status = "Running" },
                    cancellationToken);

                var review = await _reviewerAgent.ReviewAsync(
                    plan,
                    generatedFiles,
                    cancellationToken);

                var reviewMessage = review.IsValid
                    ? $"Review passed: {review.Summary}"
                    : $"Review failed: {review.Summary}";

                AddMessage(generation, AgentRole.Reviewer, reviewMessage);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "AgentMessage",
                    new
                    {
                        Role = AgentRole.Reviewer.ToString(),
                        Content = reviewMessage,
                        review.Issues,
                        review.Suggestions
                    },
                    cancellationToken);

                if (!review.IsValid)
                {
                    generation.Status = GenerationStatus.Failed;
                    generation.ErrorMessage = reviewMessage;
                    generation.CompletedAtUtc = DateTime.UtcNow;

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    await _realtimeNotifier.SendMessageAsync(
                        generation.Id,
                        "GenerationFailed",
                        new { Error = reviewMessage, review.Issues },
                        cancellationToken);

                    return;
                }

                CompleteStep(generation, AgentRole.Reviewer);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "StepCompleted",
                    new { Step = "Reviewer", Status = "Completed" },
                    cancellationToken);

                generation.Status = GenerationStatus.Completed;
                generation.CompletedAtUtc = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "GenerationCompleted",
                    new { generation.Id, generation.ProjectName },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                generation.Status = GenerationStatus.Failed;
                generation.ErrorMessage = ex.Message;
                generation.CompletedAtUtc = DateTime.UtcNow;

                AddMessage(generation, AgentRole.System, $"Generation failed: {ex.Message}");

                await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                await _realtimeNotifier.SendMessageAsync(
                    generation.Id,
                    "GenerationFailed",
                    new { Error = ex.Message },
                    CancellationToken.None);
            }
        }
        public static void CompleteStep(ProjectGeneration generation, AgentRole role)
        {
            var step = generation.Steps.FirstOrDefault(x => x.AgentRole == role);

            if (step is null)
                return;

            step.Status = StepStatus.Completed;
            step.StartedAtUtc ??= DateTime.UtcNow;
            step.CompletedAtUtc = DateTime.UtcNow;
        }

        public static void AddMessage(ProjectGeneration generation, AgentRole role, string content)
        {
            generation.Messages.Add(new AgentMessage
            {
                ProjectGenerationId = generation.Id,
                Role = role,
                Content = content
            });
        }
        public static string CleanStreamedContent(string content)
        {
            var cleaned = content.Trim();

            if (cleaned.StartsWith("```"))
            {
                var firstNewLine = cleaned.IndexOf('\n');
                var lastFence = cleaned.LastIndexOf("```", StringComparison.Ordinal);

                if (firstNewLine >= 0 && lastFence > firstNewLine)
                {
                    cleaned = cleaned[(firstNewLine + 1)..lastFence].Trim();
                }
            }

            return cleaned;
        }

        public static string InferLanguage(string path)
        {
            if (Path.GetFileName(path).Equals("Dockerfile", StringComparison.OrdinalIgnoreCase))
                return "dockerfile";

            if (Path.GetFileName(path).Equals(".gitignore", StringComparison.OrdinalIgnoreCase))
                return "text";

            if (Path.GetFileName(path).Equals("LICENSE", StringComparison.OrdinalIgnoreCase))
                return "text";

            var extension = Path.GetExtension(path).ToLowerInvariant();

            return extension switch
            {
                ".html" => "html",
                ".css" => "css",
                ".js" => "javascript",
                ".json" => "json",
                ".md" => "markdown",
                ".cs" => "csharp",
                ".razor" => "razor",
                _ => "text"
            };
        }
    }
}
