using CodingAssistant.Application.Abstractions;
using CodingAssistant.Application.GitHub;
using Octokit;

namespace CodingAssistant.Api.GitHub
{

    public sealed class GitHubExportService : IGitHubExportService
    {
        public readonly IProjectGenerationRepository _repository;

        public GitHubExportService(
            IProjectGenerationRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> PublishAsync(
            Guid generationId,
            string repositoryName,
            bool isPrivate,
            string githubToken,
            CancellationToken cancellationToken = default)
        {

            repositoryName = SanitizeRepositoryName(repositoryName);
            var generation = await _repository.GetByIdAsync(
                generationId,
                cancellationToken);

            if (generation is null)
                throw new InvalidOperationException("Generation not found.");

            var client = new GitHubClient(
                new ProductHeaderValue("MultiAgentCodingAssistant"))
            {
                Credentials = new Credentials(githubToken)
            };

            Repository repository;

            try
            {
                repository = await client.Repository.Create(
                    new NewRepository(repositoryName)
                    {
                        Private = isPrivate,
                        AutoInit = false
                    });
            }
            catch (ApiValidationException ex)
            {
                throw new InvalidOperationException(
                    $"GitHub repository '{repositoryName}' could not be created. It may already exist.",
                    ex);
            }

            foreach (var file in generation.Files)
            {
                await client.Repository.Content.CreateFile(
                    repository.Owner.Login,
                    repository.Name,
                    file.RelativePath,
                    new CreateFileRequest(
                        $"Add {file.RelativePath}",
                        file.Content));
            }

            return repository.HtmlUrl;
        }

        public static string SanitizeRepositoryName(string value)
        {
            var cleaned = new string(value
                .Trim()
                .ToLowerInvariant()
                .Select(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_'
                    ? ch
                    : '-')
                .ToArray());

            while (cleaned.Contains("--"))
                cleaned = cleaned.Replace("--", "-");

            return cleaned.Trim('-');
        }
    }
}
