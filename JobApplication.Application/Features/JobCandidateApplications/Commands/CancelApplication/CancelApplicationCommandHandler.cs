using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.JobCandidateApplications.Commands.CancelApplication;

public class CancelApplicationCommandHandler : IRequestHandler<CancelApplicationCommand, CancelApplicationResult>
{
    private readonly IRepository<JobCandidateApplication> _applicationRepository;
    private readonly IRepository<Candidate> _candidateRepository;

    public CancelApplicationCommandHandler(
        IRepository<JobCandidateApplication> applicationRepository,
        IRepository<Candidate> candidateRepository)
    {
        _applicationRepository = applicationRepository;
        _candidateRepository = candidateRepository;
    }

    public async Task<CancelApplicationResult> Handle(CancelApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Resolve candidate by UserId (or fallback to CandidateId parsing)
        var candidate = await _candidateRepository
            .Get()
            .FirstOrDefaultAsync(c => c.UserId == request.RequesterId, cancellationToken);

        int resolvedCandidateId = candidate != null
            ? candidate.Id
            : (int.TryParse(request.RequesterId, out var parsedId) ? parsedId : -1);

        if (resolvedCandidateId == -1)
            return CancelApplicationResult.Forbidden;

        // 2. Find application
        var application = await _applicationRepository
            .Get()
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application is null)
            return CancelApplicationResult.NotFound;

        // 3. Ownership check: 403 if not owner
        if (application.CandidateId != resolvedCandidateId)
            return CancelApplicationResult.Forbidden;

        // 4. Status check: reject if status is Interview+ (can only cancel Applied or UnderReview)
        if (application.JobApplicationStatus is not (JobApplicationStatus.Applied or JobApplicationStatus.UnderReview))
            return CancelApplicationResult.InvalidStatus;

        // 5. Update status and timestamp
        application.JobApplicationStatus = JobApplicationStatus.Cancelled;
        application.CancelledAt = DateTime.UtcNow;
        application.StatusUpdatedAt = DateTime.UtcNow;

        _applicationRepository.Update(application);
        await _applicationRepository.SaveChangesAsync();

        return CancelApplicationResult.Success;
    }
}
