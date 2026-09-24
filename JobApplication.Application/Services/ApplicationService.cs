using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Services;

public class ApplicationService : IApplicationService
{
    private readonly IRepository<JobCandidateApplication> _applicationRepository;
    private readonly IRepository<Candidate> _candidateRepository;
    private readonly IRepository<Job> _jobRepository;

    public ApplicationService(
        IRepository<JobCandidateApplication> applicationRepository,
        IRepository<Candidate> candidateRepository,
        IRepository<Job> jobRepository)
    {
        _applicationRepository = applicationRepository;
        _candidateRepository = candidateRepository;
        _jobRepository = jobRepository;
    }

    public async Task<CancelApplicationResult> CancelAsync(
        int applicationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Resolve the candidate that belongs to the authenticated user
        var candidate = _candidateRepository
            .Get()
            .FirstOrDefault(c => c.UserId == userId);

        if (candidate is null)
            return CancelApplicationResult.Forbidden;

        var application = _applicationRepository
            .Get()
            .FirstOrDefault(a => a.Id == applicationId);

        if (application is null)
            return CancelApplicationResult.NotFound;

        // Ownership check — token-resolved candidateId must match the application
        if (application.CandidateId != candidate.Id)
            return CancelApplicationResult.Forbidden;

        // Status check — can only cancel Applied or UnderReview
        if (application.JobApplicationStatus is not (JobApplicationStatus.Applied or JobApplicationStatus.UnderReview))
            return CancelApplicationResult.InvalidStatus;

        application.JobApplicationStatus = JobApplicationStatus.Cancelled;
        application.CancelledAt = DateTime.UtcNow;
        application.StatusUpdatedAt = DateTime.UtcNow;

        _applicationRepository.Update(application);
        await _applicationRepository.SaveChangesAsync();

        return CancelApplicationResult.Success;
    }

    public async Task<ApplyResult> ApplyAsync(
        ApplyRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Resolve candidate from the authenticated user
        var candidate = _candidateRepository
            .Get()
            .FirstOrDefault(c => c.UserId == userId);

        if (candidate is null)
            return ApplyResult.CandidateNotFound;

        // Validate the job exists
        var job = _jobRepository
            .Get()
            .FirstOrDefault(j => j.Id == request.JobId);

        if (job is null)
            return ApplyResult.JobNotFound;

        // Job must be active
        if (!job.IsActive)
            return ApplyResult.JobClosed;

        // Prevent duplicate applications
        var alreadyApplied = _applicationRepository
            .Get()
            .Any(a => a.CandidateId == candidate.Id &&
                      a.JobId == request.JobId);

        if (alreadyApplied)
            return ApplyResult.AlreadyApplied;

        var application = new JobCandidateApplication
        {
            CandidateId = candidate.Id,
            JobId = request.JobId,
            JobApplicationStatus = JobApplicationStatus.Applied,
            AppliedAt = DateTime.UtcNow,
            StatusUpdatedAt = DateTime.UtcNow
        };

        await _applicationRepository.AddAsync(application);
        await _applicationRepository.SaveChangesAsync();

        return ApplyResult.Success;
    }
}
