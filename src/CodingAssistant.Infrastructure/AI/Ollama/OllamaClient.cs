using CodingAssistant.Application.AI.Dtos;
using CodingAssistant.Application.AI.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace CodingAssistant.Infrastructure.AI.Ollama
{
    public sealed class OllamaClient : ILlmClient
    {
        public readonly HttpClient _httpClient;
        public readonly OllamaOptions _options;

        public OllamaClient(HttpClient httpClient, IOptions<OllamaOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string> ChatAsync(
            IReadOnlyList<LlmChatMessage> messages,
            CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Calling Ollama...");
            var payload = new
            {
                model = _options.Model,
                stream = false,
                messages = messages.Select(x => new
                {
                    role = x.Role,
                    content = x.Content
                })
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/api/chat",
                payload,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
            Console.WriteLine("Ollama response received.");
            return document.RootElement
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }

        public async IAsyncEnumerable<string> ChatStreamAsync(
    IReadOnlyList<LlmChatMessage> messages,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                model = _options.Model,
                stream = true,
                messages = messages.Select(x => new
                {
                    role = x.Role,
                    content = x.Content
                })
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "/api/chat",
                payload,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                using var document = JsonDocument.Parse(line);

                if (document.RootElement.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var content))
                {
                    var token = content.GetString();

                    if (!string.IsNullOrEmpty(token))
                        yield return token;
                }

                if (document.RootElement.TryGetProperty("done", out var done) &&
                    done.GetBoolean())
                {
                    yield break;
                }
            }
        }
    }
}
