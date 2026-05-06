using CodingAssistant.Application.AI.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Application.AI.Services
{
    public interface ILlmClient
    {
        Task<string> ChatAsync(
            IReadOnlyList<LlmChatMessage> messages,
            CancellationToken cancellationToken = default);
    }
}
