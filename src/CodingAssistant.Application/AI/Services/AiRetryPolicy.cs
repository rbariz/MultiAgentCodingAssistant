namespace CodingAssistant.Application.AI.Services
{
    public sealed class AiRetryPolicy : IAiRetryPolicy
    {
        public async Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> action,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            const int maxAttempts = 3;

            Exception? lastException = null;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await action(cancellationToken);
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    lastException = ex;
                    await Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken);
                }
            }

            throw new InvalidOperationException(
                $"AI operation failed after {maxAttempts} attempts: {operationName}",
                lastException);
        }
    }
}
