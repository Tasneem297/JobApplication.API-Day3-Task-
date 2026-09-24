using JobApplication.Application.Authentication;
using JobApplication.Application.Consts;
using JobApplication.Application.DTOs.Auth;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Entities.Identity;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System.Security.Cryptography;
using System.Text;

namespace JobApplication.Application.Services;

public class AuthService: IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtProvider _jwtProvider;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAuthRepository _AuthRepository;
    private readonly IRepository<Candidate> _candidateRepository;
    private readonly int _refreshTokenExpiryDays = 14;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtProvider jwtProvider,
        SignInManager<ApplicationUser> signInManager,
        IAuthRepository authRepository,
        IRepository<Candidate> candidateRepository
        )
    {
        _userManager = userManager;
        _jwtProvider = jwtProvider;
        _signInManager = signInManager;
        _AuthRepository = authRepository;
        _candidateRepository = candidateRepository;
    }

    public async Task<AuthResponse?> GetTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return null;

        var result = await _signInManager.PasswordSignInAsync(
            user,
            password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!result.Succeeded)
            return null;

        var (userRoles, userPermissions) =
            await _AuthRepository.GetUserRolesAndPermissions(user, cancellationToken);

        var (token, expiresIn) =
            _jwtProvider.GenerateToken(
                user,
                userRoles,
                userPermissions);

        var refreshToken = GenerateRefreshToken();

        var refreshTokenExpiration =
            DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresOn = refreshTokenExpiration
        });

        await _userManager.UpdateAsync(user);

        var response = new AuthResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FName,
            LastName = user.LName,
            Token = token,
            ExpiresIn = expiresIn,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpiration
        };

      

        return response;
    }

    public async Task<AuthResponse?> GetRefreshTokenAsync(
        string token,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
            return null;

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return null;


        if (user.LockoutEnd > DateTime.UtcNow)
            return null;

        var userRefreshToken =
            user.RefreshTokens
                .SingleOrDefault(x =>
                    x.Token == refreshToken &&
                    x.IsActive);

        if (userRefreshToken is null)
            return null;

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        var (userRoles, userPermissions) =
            await _AuthRepository.GetUserRolesAndPermissions(
                user,
                cancellationToken);

        var (newToken, expiresIn) =
            _jwtProvider.GenerateToken(
                user,
                userRoles,
                userPermissions);

        var newRefreshToken = GenerateRefreshToken();

        var refreshTokenExpiration =
            DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresOn = refreshTokenExpiration
        });

        await _userManager.UpdateAsync(user);

        var response = new AuthResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FName,
            LastName = user.LName,

            // IMPORTANT:
            Token = newToken,

            ExpiresIn = expiresIn,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiration = refreshTokenExpiration
        };


        return response;
    }

    public async Task<bool> RevokeRefreshTokenAsync(
        string token,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
            return false;

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return false;

        var userRefreshToken =
            user.RefreshTokens
                .SingleOrDefault(x =>
                    x.Token == refreshToken &&
                    x.IsActive);

        if (userRefreshToken is null)
            return false;

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        return true;
    }
    
    
    public async Task<bool> RegisterAsync(
        registerRequest request,
        CancellationToken cancellationToken = default)
    {
        var emailExists =
            await _userManager.Users
                .AnyAsync(
                    u => u.Email == request.Email,
                    cancellationToken);

        if (emailExists)
            return false;

        var user = request.Adapt<ApplicationUser>();

        var result =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!result.Succeeded)
            return false;

        await _userManager.AddToRoleAsync(user, request.UserType);

        // Auto-create Candidate profile for Applicants
        if (request.UserType == DefaultRoles.Applicant)
        {
            var candidate = new Candidate
            {
                UserId = user.Id,
                Name = $"{request.FirstName} {request.LastName}",
                CvUrl = string.Empty
            };

            await _candidateRepository.AddAsync(candidate);
            await _candidateRepository.SaveChangesAsync();
        }

        return true;
    }

    public static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));
    }
}