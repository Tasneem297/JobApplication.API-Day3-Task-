using JobApplication.Application.DTOs;

namespace JobApplication.Application.Interfaces;

public interface IApplicationService
{
    Task<CancelApplicationResult> CancelAsync(int applicationId, string userId, CancellationToken cancellationToken = default);
    Task<ApplyResult> ApplyAsync(ApplyRequest request, string userId, CancellationToken cancellationToken = default);
}
