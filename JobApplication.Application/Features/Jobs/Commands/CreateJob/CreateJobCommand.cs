using MediatR;

namespace JobApplication.Application.Features.Jobs.Commands.CreateJob;

public class CreateJobCommand : IRequest<int>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
