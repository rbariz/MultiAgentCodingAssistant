using CodingAssistant.Application.AI.Dtos;
using System.Text.Json;

namespace CodingAssistant.Application.AI.Services
{
    public sealed class FileGenerationAgent : IFileGenerationAgent
    {
        private readonly ILlmClient _llmClient;
        private readonly IAiJsonParser _jsonParser;
        private readonly IAiRetryPolicy _retryPolicy;

        public FileGenerationAgent(ILlmClient llmClient, IAiJsonParser jsonParser, IAiRetryPolicy retryPolicy)
        {
            _llmClient = llmClient;
            _jsonParser = jsonParser;
            _retryPolicy = retryPolicy;
        }

        public async Task<GeneratedFileAiResponse> GenerateFileAsync(
            string userPrompt,
            string? targetStack,
            ProjectPlanAiResponse plan,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            

            return await _retryPolicy.ExecuteAsync(
    async ct =>
    {
        var systemPrompt = """
        You are a senior software developer acting as a Developer Agent.

        Your task is to generate exactly one file from a project plan.

        Return ONLY valid JSON.
        Do not return markdown.
        Do not wrap the JSON in code fences.
        Do not add explanations.

        JSON schema:
        {
          "path": "index.html",
          "language": "html",
          "content": "complete file content"
        }

        Rules:
        - Generate exactly the requested file.
        - Do not generate other files.
        - Return complete runnable code.
        - Keep the file concise.
        - Use relative paths only.
        - Escape JSON strings correctly.
        """;

        var userContent = $"""
        User request:
        {userPrompt}

        Target stack:
        {targetStack ?? "html-css-js"}

        Project name:
        {plan.ProjectName}

        Project type:
        {plan.ProjectType}

        Project description:
        {plan.Description}

        All project files:
        {string.Join("\n", plan.Files)}

        Architecture notes:
        {string.Join("\n", plan.ArchitectureNotes)}

        File to generate:
        {filePath}
        """;

        var raw = await _llmClient.ChatAsync(
            [
                new LlmChatMessage { Role = "system", Content = systemPrompt },
                new LlmChatMessage { Role = "user", Content = userContent }
            ],
            cancellationToken);

        var result = _jsonParser.ParseObject<GeneratedFileAiResponse>(
            raw,
            $"file generation for {filePath}");

        if (result is null || string.IsNullOrWhiteSpace(result.Content))
            throw new InvalidOperationException($"Developer agent did not return valid content for {filePath}.");

        result.Path = NormalizeRelativePath(
            string.IsNullOrWhiteSpace(result.Path) ? filePath : result.Path);

        result.Language = string.IsNullOrWhiteSpace(result.Language)
            ? InferLanguage(result.Path)
            : result.Language.Trim().ToLowerInvariant();

        return result;
    },
    $"file generation for {filePath}",
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

        private static string NormalizeRelativePath(string path)
        {
            return path
                .Replace("\\", "/")
                .Trim()
                .TrimStart('/');
        }

        private static string InferLanguage(string path)
        {
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
