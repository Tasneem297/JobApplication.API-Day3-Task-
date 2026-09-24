using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob;

public class CloseJobCommandHandler : IRequestHandler<CloseJobCommand, CloseJobResult>
{
    private readonly IRepository<Job> _jobRepository;

    public CloseJobCommandHandler(IRepository<Job> jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<CloseJobResult> Handle(CloseJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.Get().FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);

        if (job is null)
            return CloseJobResult.NotFound;

        // Ownership check: only the recruiter who created the job can close it
        if (job.RecruiterId != request.RecruiterId)
            return CloseJobResult.Forbidden;

        if (!job.IsActive)
            return CloseJobResult.AlreadyClosed;

        job.IsActive = false;
        job.ClosedAt = DateTime.UtcNow;

        _jobRepository.Update(job);
        await _jobRepository.SaveChangesAsync();

        return CloseJobResult.Success;
    }
}
