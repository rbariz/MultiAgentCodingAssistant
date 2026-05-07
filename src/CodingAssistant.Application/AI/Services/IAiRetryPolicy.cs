namespace CodingAssistant.Application.AI.Services
{
    public interface IAiRetryPolicy
    {
        Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> action,
            string operationName,
            CancellationToken cancellationToken = default);
    }
}
