using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobApplication.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return Unauthorized();

        var result = await _applicationService.CancelAsync(id, userId, cancellationToken);

        return result switch
        {
            CancelApplicationResult.Success       => NoContent(),
            CancelApplicationResult.NotFound      => NotFound($"Application {id} was not found."),
            CancelApplicationResult.Forbidden     => Forbid(),
            CancelApplicationResult.InvalidStatus => BadRequest("Application can only be cancelled when status is Applied or UnderReview."),
            _                                     => StatusCode(500)
        };
    }

    [HttpPost]
    public async Task<IActionResult> Apply([FromBody] ApplyRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return Unauthorized();

        var result = await _applicationService.ApplyAsync(request, userId, cancellationToken);

        return result switch
        {
            ApplyResult.Success           => StatusCode(201),
            ApplyResult.CandidateNotFound => NotFound("No candidate profile found for the current user."),
            ApplyResult.JobNotFound       => NotFound($"Job {request.JobId} was not found."),
            ApplyResult.JobClosed         => BadRequest("Cannot apply to a closed job."),
            ApplyResult.AlreadyApplied    => Conflict("You have already applied to this job."),
            _                             => StatusCode(500)
        };
    }
}
