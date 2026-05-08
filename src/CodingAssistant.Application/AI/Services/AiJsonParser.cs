using System.Text.Json;

namespace CodingAssistant.Application.AI.Services
{
    public sealed class AiJsonParser : IAiJsonParser
    {
        public T ParseObject<T>(string raw, string context)
        {
            var json = ExtractJson(raw);
            json = RepairBacktickContent(json);

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

        public static string ExtractJson(string raw)
        {
            var text = raw.Trim();

            var firstObject = text.IndexOf('{');
            var lastObject = text.LastIndexOf('}');

            if (firstObject >= 0 && lastObject > firstObject)
                return text[firstObject..(lastObject + 1)];

            return text;
        }
        public static string RepairBacktickContent(string json)
        {
            const string marker = "\"content\":";

            var markerIndex = json.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

            if (markerIndex < 0)
                return json;

            var afterMarkerIndex = markerIndex + marker.Length;

            while (afterMarkerIndex < json.Length && char.IsWhiteSpace(json[afterMarkerIndex]))
                afterMarkerIndex++;

            if (afterMarkerIndex >= json.Length || json[afterMarkerIndex] != '`')
                return json;

            var contentStart = afterMarkerIndex + 1;
            var contentEnd = json.LastIndexOf('`');

            if (contentEnd <= contentStart)
                return json;

            var rawContent = json[contentStart..contentEnd];

            var encodedContent = System.Text.Json.JsonSerializer.Serialize(rawContent);

            return json[..afterMarkerIndex] + encodedContent + json[(contentEnd + 1)..];
        }
    }
}
