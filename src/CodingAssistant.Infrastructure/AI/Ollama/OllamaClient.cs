using CodingAssistant.Application.AI.Dtos;
using CodingAssistant.Application.AI.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace CodingAssistant.Infrastructure.AI.Ollama
{
    public sealed class OllamaClient : ILlmClient
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaOptions _options;

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
    }
}
