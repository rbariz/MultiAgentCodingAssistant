using CodingAssistant.Application.AI.Dtos;
using System.Text.Json;

namespace CodingAssistant.Application.AI.Services
{
    public sealed class ProjectPlannerAgent : IProjectPlannerAgent
    {
        private readonly ILlmClient _llmClient;
        private readonly IAiJsonParser _jsonParser;
        private readonly IAiRetryPolicy _retryPolicy;

        public ProjectPlannerAgent(ILlmClient llmClient, IAiJsonParser jsonParser, IAiRetryPolicy retryPolicy)
        {
            _llmClient = llmClient;
            _jsonParser = jsonParser;
            _retryPolicy = retryPolicy;
        }

        public async Task<ProjectPlanAiResponse> CreatePlanAsync(
            string userPrompt,
            string? targetStack,
            CancellationToken cancellationToken = default)
        {
            


            return await _retryPolicy.ExecuteAsync(
                        async ct =>
                        {
                            var systemPrompt = """
        You are a senior software architect acting as a Planner Agent.

        Your task is to analyze the user request and produce a small project plan.

        Return ONLY valid JSON.
        Do not return markdown.
        Do not wrap the JSON in code fences.
        Do not add explanations.

        JSON schema:
        {
          "projectName": "kebab-case-project-name",
          "projectType": "web-app",
          "description": "short project description",
          "files": [
            "index.html",
            "style.css",
            "script.js"
          ],
          "architectureNotes": [
            "Use vanilla JavaScript",
            "Keep the project small and runnable"
          ]
        }

        Rules:
        - Use relative file paths only.
        - For html-css-js projects, prefer index.html, style.css and script.js.
        - Keep the project small.
        - Do not generate file contents here.
        - Only return the plan.
        """;

                            var userContent = $"""
        User request:
        {userPrompt}

        Target stack:
        {targetStack ?? "html-css-js"}
        """;

                            var raw = await _llmClient.ChatAsync(
                                [
                                    new LlmChatMessage { Role = "system", Content = systemPrompt },
                new LlmChatMessage { Role = "user", Content = userContent }
                                ],
                                cancellationToken);

                            var result = _jsonParser.ParseObject<ProjectPlanAiResponse>(
                                    raw,
                                    "project planning");

                            if (result is null || result.Files.Count == 0)
                                throw new InvalidOperationException("The planner agent did not return a valid project plan.");

                            result.ProjectName = SanitizeProjectName(result.ProjectName);

                            result.Files = result.Files
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Select(NormalizeRelativePath)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToList();

                            return result;
                        },
                        "project planning",
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

        private static string SanitizeProjectName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "generated-app";

            var cleaned = new string(value
                .Trim()
                .ToLowerInvariant()
                .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
                .ToArray());

            while (cleaned.Contains("--"))
                cleaned = cleaned.Replace("--", "-");

            return cleaned.Trim('-');
        }

        private static string NormalizeRelativePath(string path)
        {
            return path
                .Replace("\\", "/")
                .Trim()
                .TrimStart('/');
        }
    }
}
