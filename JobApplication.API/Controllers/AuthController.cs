using JobApplication.Application.DTOs.Auth;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Services;
using JobApplication.Application.Settings;
using JobApplication.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JobApplication.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService, IConfiguration configuration,
                            IOptions<JwtOptions> JwtOptions,
                            
                            UserManager<ApplicationUser> userManager) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IConfiguration _Configuration = configuration;
    private readonly UserManager<ApplicationUser> _UserManager = userManager;
    private readonly JwtOptions _JwtOptions = JwtOptions.Value;



    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginRequestUser loginRequest,
        CancellationToken cancellationToken)
    {
        var authResponse = await _authService.GetTokenAsync(
            loginRequest.Email,
            loginRequest.Password,
            cancellationToken);

        if (authResponse is null)
            return Unauthorized("Invalid email or password.");

        return Ok(authResponse);
    }


    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var authResponse = await _authService.GetRefreshTokenAsync(
            request.Token,
            request.RefreshToken,
            cancellationToken);

        if (authResponse is null)
            return Unauthorized("Invalid or expired refresh token.");

        return Ok(authResponse);
    }


    [HttpPost("revoke-refresh-token")]
    public async Task<IActionResult> RevokeRefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var success = await _authService.RevokeRefreshTokenAsync(
            request.Token,
            request.RefreshToken,
            cancellationToken);

      

        return Ok();
    }


    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] registerRequest registerRequest,
        CancellationToken cancellationToken)
    {
        var success = await _authService.RegisterAsync(
            registerRequest,
            cancellationToken);

        if (!success)
            return BadRequest("Registration failed.");

        return Ok();
    }






}
