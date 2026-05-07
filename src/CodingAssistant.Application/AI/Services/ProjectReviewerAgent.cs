using CodingAssistant.Application.AI.Dtos;
using CodingAssistant.Domain.Entities;
using System.Text.Json;

namespace CodingAssistant.Application.AI.Services
{
    public sealed class ProjectReviewerAgent : IProjectReviewerAgent
    {
        private readonly ILlmClient _llmClient;
        private readonly IAiJsonParser _jsonParser;
        private readonly IAiRetryPolicy _retryPolicy;

        public ProjectReviewerAgent(ILlmClient llmClient, IAiJsonParser jsonParser, IAiRetryPolicy retryPolicy)
        {
            _llmClient = llmClient;
            _jsonParser = jsonParser;
            _retryPolicy = retryPolicy;
        }

        public async Task<ProjectReviewAiResponse> ReviewAsync(
            ProjectPlanAiResponse plan,
            IReadOnlyList<GeneratedFile> files,
            CancellationToken cancellationToken = default)
        {
            

            return await _retryPolicy.ExecuteAsync(
    async ct =>
    {
        var systemPrompt = """
        You are a senior code reviewer acting as a Reviewer Agent.

        Review the generated project and decide if it is runnable and consistent.

        Return ONLY valid JSON.
        Do not return markdown.
        Do not wrap JSON in code fences.

        JSON schema:
        {
          "isValid": true,
          "summary": "short review summary",
          "issues": [],
          "suggestions": []
        }

        Validation criteria:
        - Required files from the plan must be present.
        - HTML/CSS/JS references should be consistent.
        - Code should be simple and runnable.
        - Do not reject for minor style issues.
        - Reject only if the project is clearly broken.
        """;

        var filesSummary = files
            .OrderBy(x => x.Order)
            .Select(x => $"""
            FILE: {x.RelativePath}
            LANGUAGE: {x.Language}
            CONTENT:
            {Truncate(x.Content, 6000)}
            """);

        var userContent = $"""
        Project plan:
        Name: {plan.ProjectName}
        Type: {plan.ProjectType}
        Description: {plan.Description}

        Planned files:
        {string.Join("\n", plan.Files)}

        Generated files:
        {string.Join("\n\n", filesSummary)}
        """;

        var raw = await _llmClient.ChatAsync(
            [
                new LlmChatMessage { Role = "system", Content = systemPrompt },
                new LlmChatMessage { Role = "user", Content = userContent }
            ],
            cancellationToken);

        var result = _jsonParser.ParseObject<ProjectReviewAiResponse>(
            raw,
            "project review");

        if (result is null)
            throw new InvalidOperationException("Reviewer agent did not return a valid review.");

        return result;
    },
    "project review",
    cancellationToken);
        }

        //private static string ExtractJson(string raw)
        //{
        //    var text = raw.Trim();
        //    var firstBrace = text.IndexOf('{');
        //    var lastBrace = text.LastIndexOf('}');

        //    if (firstBrace >= 0 && lastBrace > firstBrace)
        //        return text[firstBrace..(lastBrace + 1)];

        //    return text;
        //}

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;

            return value[..maxLength] + "\n...[truncated]";
        }
    }
}
