using System.ComponentModel.DataAnnotations;


namespace JobApplication.Application.Settings;

public class JwtOptions
{
    public static string SectionName => "JWT";
    [Required]
    public string Key { get; init; } = string.Empty;
    [Required]
    public string Issuer { get; init; } = string.Empty;
    [Required]
    public string Audience { get; init; } = string.Empty;
    [Required]
    [Range(1, int.MaxValue)]    
    public int ExpiryMinutes { get; init; }
}
