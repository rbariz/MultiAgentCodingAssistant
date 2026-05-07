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
        private readonly IProjectPlannerAgent _plannerAgent;
        private readonly IFileGenerationAgent _fileGenerationAgent;
        private readonly IProjectReviewerAgent _reviewerAgent;

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

                    var aiFile = await _fileGenerationAgent.GenerateFileAsync(
                        generation.UserPrompt,
                        generation.TargetStack,
                        plan,
                        plannedFile,
                        cancellationToken);

                    var generatedFile = new GeneratedFile
                    {
                        ProjectGenerationId = generation.Id,
                        RelativePath = aiFile.Path,
                        FileName = Path.GetFileName(aiFile.Path),
                        Language = aiFile.Language,
                        Kind = GeneratedFileKind.SourceCode,
                        Order = order++,
                        Content = aiFile.Content
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
