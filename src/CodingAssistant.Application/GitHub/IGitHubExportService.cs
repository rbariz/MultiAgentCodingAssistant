using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Application.GitHub
{
    public interface IGitHubExportService
    {
        Task<string> PublishAsync(
            Guid generationId,
            string repositoryName,
            bool isPrivate,
            string githubToken,
            CancellationToken cancellationToken = default);
    }
}
