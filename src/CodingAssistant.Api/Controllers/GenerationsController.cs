using CodingAssistant.Application.AI.Dtos;
using CodingAssistant.Application.AI.Services;
using CodingAssistant.Application.Generations.Dtos;
using CodingAssistant.Application.Generations.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CodingAssistant.Api.Controllers
{

    [ApiController]
    [Route("api/generations")]
    public sealed class GenerationsController : ControllerBase
    {
        private readonly IProjectGenerationService _service;
        private readonly IProjectGenerationOrchestrator _orchestrator;
        private readonly IProjectExportService _exportService;
        private readonly ILlmClient _llmClient;

        public GenerationsController(IProjectGenerationService service,
            IProjectGenerationOrchestrator orchestrator, 
            IProjectExportService exportService,
            ILlmClient llmClient)
        {
            _service = service;
            _orchestrator = orchestrator;
            _exportService = exportService;
            _llmClient = llmClient;
        }

        [HttpPost]
        public async Task<ActionResult<ProjectGenerationDto>> Create(
            [FromBody] CreateGenerationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            await _orchestrator.RunAsync(result.Id, cancellationToken);



            var completed = await _service.GetByIdAsync(result.Id, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                completed);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProjectGenerationDto>> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);

            if (result is null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProjectGenerationDto>>> GetRecent(
            [FromQuery] int take = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetRecentAsync(take, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}/export")]
        public async Task<IActionResult> Export(
    Guid id,
    CancellationToken cancellationToken)
        {
            var generation = await _service.GetByIdAsync(id, cancellationToken);

            if (generation is null)
                return NotFound();

            var zipBytes = await _exportService.ExportZipAsync(id, cancellationToken);

            var fileName = string.IsNullOrWhiteSpace(generation.ProjectName)
                ? "generated-project.zip"
                : $"{generation.ProjectName}.zip";

            return File(zipBytes, "application/zip", fileName);
        }

        [HttpGet("llm-test")]
        public async Task<ActionResult<string>> TestLlm(CancellationToken cancellationToken)
        {
            var result = await _llmClient.ChatAsync(
                [
                    new LlmChatMessage
            {
                Role = "user",
                Content = "Say hello from Ollama in one short sentence."
            }
                ],
                cancellationToken);

            return Ok(result);
        }
    }
}
