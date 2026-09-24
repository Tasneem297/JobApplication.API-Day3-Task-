namespace JobApplication.Application.DTOs.Auth;

public record RefreshTokenRequest(
    string Token,
    string RefreshToken
    );

