using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace JobApplication.Domain.Entities.Identity
{
    public class ApplicationUser:IdentityUser

    {
        [Required]
        public string FName { get; set; }
        [Required]
        public string LName { get; set; }

        [Required]
        public string Gender { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
