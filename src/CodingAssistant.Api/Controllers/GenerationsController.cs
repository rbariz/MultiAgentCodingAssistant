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

        public GenerationsController(IProjectGenerationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<ProjectGenerationDto>> Create(
            [FromBody] CreateGenerationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
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
    }
}
