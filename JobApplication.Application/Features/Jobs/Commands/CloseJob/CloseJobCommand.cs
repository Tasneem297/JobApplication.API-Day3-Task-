using JobApplication.Application.DTOs;
using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CloseJob;

public record CloseJobCommand(int JobId, string RecruiterId) : IRequest<CloseJobResult>;
