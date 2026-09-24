using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobApplication.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _JobService;

        public JobsController(IJobService jobService)
        {
            _JobService = jobService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(CreateJobDto createJobDto)
        {
            var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (recruiterId is null)
                return Unauthorized();

            var id = await _JobService.CreateAsync(createJobDto, recruiterId);
            return Ok(new { id });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var jobs = _JobService.GetAll();

            if (!jobs.Any())
                return NoContent();

            return Ok(jobs);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var job = _JobService.GetById(id);

            if (job is null)
                return NotFound();

            return Ok(job);
        }

        [HttpPut("{id:int}/close")]
        [Authorize]
        public async Task<IActionResult> Close(int id, CancellationToken cancellationToken)
        {
            var recruiterId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (recruiterId is null)
                return Unauthorized();

            var result = await _JobService.CloseAsync(id, recruiterId, cancellationToken);

            return result switch
            {
                CloseJobResult.Success      => NoContent(),
                CloseJobResult.NotFound     => NotFound($"Job {id} was not found."),
                CloseJobResult.Forbidden    => Forbid(),
                CloseJobResult.AlreadyClosed => Conflict("Job is already closed."),
                _                           => StatusCode(500)
            };
        }
    }
}
