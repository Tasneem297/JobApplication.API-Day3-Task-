using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobApplication.Application.Services
{
    public class JobService:IJobService
    {
        private readonly IRepository<Job> _jobRepository;

        public JobService(IRepository<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<int> CreateAsync(CreateJobDto createJobDto, string recruiterId)
        {   
            var job = new Job()
            {
                Title = createJobDto.Title,
                Description = createJobDto.Description,
                IsActive = true,
                RecruiterId = recruiterId
            };
            await _jobRepository.AddAsync(job);
            await _jobRepository.SaveChangesAsync();

            return job.Id; 
        }
        public IEnumerable<Job> GetAll()
        {
            var jobs = _jobRepository.Get().ToList();
            return jobs;
        }

        public Job? GetById(int id)
        {
            var job = _jobRepository.Get().FirstOrDefault(j => j.Id == id);
            return job;
        }

        public async Task<CloseJobResult> CloseAsync(
            int jobId,
            string recruiterId,
            CancellationToken cancellationToken = default)
        {
            var job = _jobRepository.Get().FirstOrDefault(j => j.Id == jobId);

            if (job is null)
                return CloseJobResult.NotFound;

            // Ownership check — only the recruiter who created the job can close it
            if (job.RecruiterId != recruiterId)
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
}
