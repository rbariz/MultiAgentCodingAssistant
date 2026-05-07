namespace CodingAssistant.Application.AI.Services
{
    public interface IAiJsonParser
    {
        T ParseObject<T>(string raw, string context);
    }
}
