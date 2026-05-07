using System.Text.Json;

namespace CodingAssistant.Application.AI.Services
{
    public sealed class AiJsonParser : IAiJsonParser
    {
        public T ParseObject<T>(string raw, string context)
        {
            var json = ExtractJson(raw);

            try
            {
                var result = JsonSerializer.Deserialize<T>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true
                    });

                if (result is null)
                    throw new InvalidOperationException($"Empty JSON result for {context}.");

                return result;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Invalid JSON returned by AI for {context}. Raw response: {raw}",
                    ex);
            }
        }

        private static string ExtractJson(string raw)
        {
            var text = raw.Trim();

            var firstObject = text.IndexOf('{');
            var lastObject = text.LastIndexOf('}');

            if (firstObject >= 0 && lastObject > firstObject)
                return text[firstObject..(lastObject + 1)];

            return text;
        }
    }
}
