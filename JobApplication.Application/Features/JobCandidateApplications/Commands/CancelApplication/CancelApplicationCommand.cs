using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.JobCandidateApplications.Commands.CancelApplication;

public record CancelApplicationCommand(int ApplicationId, string RequesterId) : IRequest<CancelApplicationResult>;
