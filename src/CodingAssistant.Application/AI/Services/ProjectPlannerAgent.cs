using CodingAssistant.Application.AI.Dtos;
using CodingAssistant.Application.AI.Prompts;
using System.Text.Json;

namespace CodingAssistant.Application.AI.Services
{
    public sealed class ProjectPlannerAgent : IProjectPlannerAgent
    {
        public readonly ILlmClient _llmClient;
        public readonly IAiJsonParser _jsonParser;
        public readonly IAiRetryPolicy _retryPolicy;

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
                            var systemPrompt = AiPromptTemplates.PlannerSystemPrompt;

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

                            if (!result.Files.Any(x => x.Equals("README.md", StringComparison.OrdinalIgnoreCase)))
                            {
                                result.Files.Add("README.md");
                            }
                            if (!result.Files.Any(x => x.Equals("Dockerfile", StringComparison.OrdinalIgnoreCase)))
                            {
                                result.Files.Add("Dockerfile");
                            }

                            if (!result.Files.Any(x => x.Equals(".gitignore", StringComparison.OrdinalIgnoreCase)))
                            {
                                result.Files.Add(".gitignore");
                            }

                            if (!result.Files.Any(x => x.Equals("LICENSE", StringComparison.OrdinalIgnoreCase)))
                            {
                                result.Files.Add("LICENSE");
                            }

                            return result;
                        },
                        "project planning",
                        cancellationToken);
        }


        public static string SanitizeProjectName(string? value)
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

        public static string NormalizeRelativePath(string path)
        {
            return path
                .Replace("\\", "/")
                .Trim()
                .TrimStart('/');
        }
    }
}
