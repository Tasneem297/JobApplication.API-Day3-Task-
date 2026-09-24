
using JobApplication.Application.DTOs.Auth;
using JobApplication.Domain.Entities.Identity;
using Mapster;

namespace JobApplication.Application.Mapping;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<registerRequest, ApplicationUser>()
            .Map(dest => dest.FName, src => src.FirstName)
            .Map(dest => dest.LName, src => src.LastName);
    }
}