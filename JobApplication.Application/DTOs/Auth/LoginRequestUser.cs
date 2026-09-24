namespace JobApplication.Application.DTOs.Auth;

public record LoginRequestUser
(
    string Email,
    string Password
    );