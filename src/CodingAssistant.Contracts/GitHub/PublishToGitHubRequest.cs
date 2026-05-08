using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingAssistant.Contracts.GitHub
{
    public sealed class PublishToGitHubRequest
    {
        public Guid GenerationId { get; set; }

        public string RepositoryName { get; set; } = string.Empty;

        public bool IsPrivate { get; set; } = true;

        public string GitHubToken { get; set; } = string.Empty;
    }
}
