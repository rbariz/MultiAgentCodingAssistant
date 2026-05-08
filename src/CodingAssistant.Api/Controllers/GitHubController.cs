using CodingAssistant.Application.GitHub;
using CodingAssistant.Contracts.GitHub;
using Microsoft.AspNetCore.Mvc;

namespace CodingAssistant.Api.Controllers
{
    [ApiController]
    [Route("api/github")]
    public sealed class GitHubController : ControllerBase
    {
        public readonly IGitHubExportService _gitHubExportService;

        public GitHubController(
            IGitHubExportService gitHubExportService)
        {
            _gitHubExportService = gitHubExportService;
        }

        [HttpPost("publish")]
        public async Task<ActionResult<PublishToGitHubResponse>> Publish(
            PublishToGitHubRequest request,
            CancellationToken cancellationToken)
        {

            if (string.IsNullOrWhiteSpace(request.GitHubToken))
                return BadRequest("GitHub token is required.");

            if (string.IsNullOrWhiteSpace(request.RepositoryName))
                return BadRequest("Repository name is required.");

            var url = await _gitHubExportService.PublishAsync(
                request.GenerationId,
                request.RepositoryName,
                request.IsPrivate,
                request.GitHubToken,
                cancellationToken);

            return Ok(new PublishToGitHubResponse
            {
                RepositoryUrl = url
            });
        }
    }
}
