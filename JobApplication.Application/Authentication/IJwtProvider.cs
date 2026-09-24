using JobApplication.Domain.Entities.Identity;
namespace JobApplication.Application.Authentication;

public interface IJwtProvider
{
    public string? ValidateToken(string token); 
    (string token,int expiresIn) GenerateToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions);
}
