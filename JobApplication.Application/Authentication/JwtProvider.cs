
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JobApplication.Application.Settings;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using JobApplication.Domain.Entities.Identity;

namespace JobApplication.Application.Authentication;

public class JwtProvider(IOptions<JwtOptions> options) : IJwtProvider

{
    private readonly JwtOptions _Options = options.Value;

    public (string token, int expiresIn) GenerateToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        Claim[] claims = [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(nameof(roles), JsonSerializer.Serialize(roles), JsonClaimValueTypes.JsonArray),
            new(nameof(permissions), JsonSerializer.Serialize(permissions), JsonClaimValueTypes.JsonArray) //unique identifier for the token
        ];
        var SymmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Options.Key));
        var SigningCredentials = new SigningCredentials(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256); // algorithm to sign the token to ensure its integrity
        var expiresIn = _Options.ExpiryMinutes;
        var token = new JwtSecurityToken(
            issuer: _Options.Issuer,
            audience: _Options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresIn),
            signingCredentials: SigningCredentials
        );
        return (token: new JwtSecurityTokenHandler().WriteToken(token),// transform token object to string
            expiresIn: expiresIn * 60);
    }

    public string? ValidateToken(string token)
    {
        var TokenHandler = new JwtSecurityTokenHandler();
        var SymmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Options.Key));
        try
        {
            //encoding
            TokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                IssuerSigningKey = SymmetricSecurityKey,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            } , out SecurityToken ValidatedToken);
            var JwtToken = (JwtSecurityToken) ValidatedToken;
            return JwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
        }
        catch
        {
            return null;
        }
    }


}

