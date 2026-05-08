using CodingAssistant.Application.AI.Dtos;
using CodingAssistant.Application.AI.Prompts;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace CodingAssistant.Application.AI.Services
{
    public sealed class FileGenerationAgent : IFileGenerationAgent
    {
        public readonly ILlmClient _llmClient;
        public readonly IAiJsonParser _jsonParser;
        public readonly IAiRetryPolicy _retryPolicy;

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

                        if (ShouldGenerateRaw(filePath))
                        {
                            var content = await GenerateRawFileContentAsync(
                                userPrompt,
                                targetStack,
                                plan,
                                filePath,
                                cancellationToken);

                            return new GeneratedFileAiResponse
                            {
                                Path = NormalizeRelativePath(filePath),
                                Language = InferLanguage(filePath),
                                Content = content
                            };
                        }
                        var systemPrompt = AiPromptTemplates.FileGenerationSystemPrompt;

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

                        result.Path = NormalizeRelativePath(filePath);

                        result.Language = string.IsNullOrWhiteSpace(result.Language)
                            ? InferLanguage(result.Path)
                            : result.Language.Trim().ToLowerInvariant();

                        return result;
                    },
                    $"file generation for {filePath}",
                    cancellationToken);
        }

        public async IAsyncEnumerable<string> StreamFilePreviewAsync(
    string userPrompt,
    string? targetStack,
    ProjectPlanAiResponse plan,
    string filePath,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var messages = new List<LlmChatMessage>
    {
        new()
        {
            Role = "system",
            Content = """
            You are a coding assistant.
            Briefly explain what you are going to generate for the requested file.
            Do not generate the full file content.
            Keep it short.
            """
        },
        new()
        {
            Role = "user",
            Content = $"""
            Project: {plan.ProjectName}
            File: {filePath}
            User request: {userPrompt}
            Target stack: {targetStack}
            """
        }
    };

            await foreach (var token in _llmClient.ChatStreamAsync(messages, cancellationToken))
            {
                yield return token;
            }
        }

        public async IAsyncEnumerable<string> StreamFileContentAsync(
    string userPrompt,
    string? targetStack,
    ProjectPlanAiResponse plan,
    string filePath,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var messages = new List<LlmChatMessage>
    {
        new()
        {
            Role = "system",
            Content = """
            You are a senior software developer.

            Generate ONLY the raw file content for the requested file.

            Rules:
            - Do not return JSON.
            - Do not return markdown fences.
            - Do not explain.
            - Do not add file path headers.
            - Return only the exact file content.
            """
        },
        new()
        {
            Role = "user",
            Content = $"""
            User request:
            {userPrompt}

            Target stack:
            {targetStack ?? "html-css-js"}

            Project name:
            {plan.ProjectName}

            Project description:
            {plan.Description}

            All files:
            {string.Join("\n", plan.Files)}

            Architecture notes:
            {string.Join("\n", plan.ArchitectureNotes)}

            File to generate:
            {filePath}
            """
        }
    };

            await foreach (var token in _llmClient.ChatStreamAsync(messages, cancellationToken))
            {
                yield return token;
            }
        }


        public static string NormalizeRelativePath(string path)
        {
            return path
                .Replace("\\", "/")
                .Trim()
                .TrimStart('/');
        }

        public static string InferLanguage(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            
            if (Path.GetFileName(path).Equals("Dockerfile", StringComparison.OrdinalIgnoreCase))
                return "dockerfile";

            if (Path.GetFileName(path).Equals(".gitignore", StringComparison.OrdinalIgnoreCase))
                return "text";

            if (Path.GetFileName(path).Equals("LICENSE", StringComparison.OrdinalIgnoreCase))
                return "text";

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

        public async Task<string> GenerateRawFileContentAsync(
    string userPrompt,
    string? targetStack,
    ProjectPlanAiResponse plan,
    string filePath,
    CancellationToken cancellationToken)
        {
            var messages = new List<LlmChatMessage>
    {
        new()
        {
            Role = "system",
            Content = """
            You are a senior software developer.

            Generate ONLY the raw file content.

            Rules:
            - Do not return JSON.
            - Do not wrap the output in markdown fences.
            - Do not add explanations.
            - Do not add file path headers.
            - Return only the exact file content.
            """
        },
        new()
        {
            Role = "user",
            Content = $"""
            User request:
            {userPrompt}

            Target stack:
            {targetStack ?? "html-css-js"}

            Project name:
            {plan.ProjectName}

            Project description:
            {plan.Description}

            All files:
            {string.Join("\n", plan.Files)}

            File to generate:
            {filePath}
            """
        }
    };

            var raw = await _llmClient.ChatAsync(messages, cancellationToken);

            return CleanRawFileContent(raw);
        }

        public static bool ShouldGenerateRaw(string path)
        {
            var fileName = Path.GetFileName(path);

            return fileName.Equals("README.md", StringComparison.OrdinalIgnoreCase)
                || fileName.Equals("Dockerfile", StringComparison.OrdinalIgnoreCase)
                || fileName.Equals(".gitignore", StringComparison.OrdinalIgnoreCase)
                || fileName.Equals("LICENSE", StringComparison.OrdinalIgnoreCase);
        }

        public static string CleanRawFileContent(string content)
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
    }
}
