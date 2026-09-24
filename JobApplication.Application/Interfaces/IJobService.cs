using JobApplication.Application.DTOs;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces;

public interface IJobService
{
    Task<int> CreateAsync(CreateJobDto createJobDto, string recruiterId);
    IEnumerable<Job> GetAll();
    Job? GetById(int id);
    Task<CloseJobResult> CloseAsync(int jobId, string recruiterId, CancellationToken cancellationToken = default);
}
