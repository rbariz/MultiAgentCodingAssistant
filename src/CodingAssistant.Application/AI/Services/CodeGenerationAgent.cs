using CodingAssistant.Application.AI.Dtos;
using System.Text.Json;

namespace CodingAssistant.Application.AI.Services
{
    public sealed class CodeGenerationAgent : ICodeGenerationAgent
    {
        private readonly ILlmClient _llmClient;

        public CodeGenerationAgent(ILlmClient llmClient)
        {
            _llmClient = llmClient;
        }

        public async Task<GeneratedProjectAiResponse> GenerateAsync(
            string userPrompt,
            string? targetStack,
            CancellationToken cancellationToken = default)
        {
            var systemPrompt = """
        You are a senior software engineer and coding agent.

        Your task is to generate a small runnable software project from the user's request.

        Return ONLY valid JSON.
        Do not return markdown.
        Do not wrap the JSON in ```json.
        Do not add explanations.

        JSON schema:
        {
          "projectName": "kebab-case-project-name",
          "files": [
            {
              "path": "index.html",
              "language": "html",
              "content": "file content here"
            }
          ]
        }

        Rules:
        - Generate complete runnable files.
        - Use relative file paths only.
        - Do not use absolute paths.
        - For html-css-js projects, generate index.html, style.css and script.js.
        - Keep the project small and understandable.
        - Escape JSON strings correctly.
        - Keep files short.
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

            var json = ExtractJson(raw);

            var result = JsonSerializer.Deserialize<GeneratedProjectAiResponse>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result is null || result.Files.Count == 0)
                throw new InvalidOperationException("The LLM did not return a valid project.");

            result.ProjectName = SanitizeProjectName(result.ProjectName);

            result.Files = result.Files
                .Where(x => !string.IsNullOrWhiteSpace(x.Path))
                .Where(x => !string.IsNullOrWhiteSpace(x.Content))
                .Select(x =>
                {
                    x.Path = NormalizeRelativePath(x.Path);
                    return x;
                })
                .ToList();

            return result;
        }

        private static string ExtractJson(string raw)
        {
            var text = raw.Trim();

            if (text.StartsWith("```"))
            {
                var firstBrace = text.IndexOf('{');
                var lastBrace = text.LastIndexOf('}');

                if (firstBrace >= 0 && lastBrace > firstBrace)
                    return text[firstBrace..(lastBrace + 1)];
            }

            return text;
        }

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
