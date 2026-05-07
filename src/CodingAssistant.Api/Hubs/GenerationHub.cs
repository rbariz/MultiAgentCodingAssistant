using Microsoft.AspNetCore.SignalR;

namespace CodingAssistant.Api.Hubs
{
    public sealed class GenerationHub : Hub
    {
        public async Task JoinGeneration(Guid generationId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                generationId.ToString());
        }
    }
}
