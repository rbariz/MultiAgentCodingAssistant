using CodingAssistant.Api.Hubs;
using CodingAssistant.Application.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace CodingAssistant.Api.Realtime
{

    public sealed class SignalRGenerationRealtimeNotifier : IGenerationRealtimeNotifier
    {
        public readonly IHubContext<GenerationHub> _hubContext;

        public SignalRGenerationRealtimeNotifier(
            IHubContext<GenerationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendMessageAsync(
            Guid generationId,
            string type,
            object payload,
            CancellationToken cancellationToken = default)
        {
            await _hubContext.Clients
                .Group(generationId.ToString())
                .SendAsync(type, payload, cancellationToken);
        }
    }
}
