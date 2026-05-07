using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Application.Realtime
{
    public interface IGenerationRealtimeNotifier
    {
        Task SendMessageAsync(
            Guid generationId,
            string type,
            object payload,
            CancellationToken cancellationToken = default);
    }
}
