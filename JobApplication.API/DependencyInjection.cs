using JobApplication.Application.Authentication;
using JobApplication.Application.Interfaces;
using JobApplication.Application.Mapping;
using JobApplication.Application.Services;
using JobApplication.Application.Settings;
using JobApplication.Infrastructure.Repositories;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
namespace JobApplication.API;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Mapster and scan MappingConfigurations : IRegister
        var mapsterConfig = TypeAdapterConfig.GlobalSettings;
        mapsterConfig.Scan(typeof(MappingConfigurations).Assembly);
        services.AddSingleton(mapsterConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddHttpContextAccessor()
            .AddAuthConfig(configuration)
            .AddSwaggerServices(); 
        return services;
    }
    private static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
    {
        //JWT Configurations
        // Read the "Jwt" section from appsettings.json and map it to JwtOptions class
        var settings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        // Register JwtOptions with the Options pattern
        services.AddOptions<JwtOptions>()
            // Binds the "Jwt" section from appsettings.json to JwtOptions
            .BindConfiguration(JwtOptions.SectionName)
            // Validates data annotations in JwtOptions (e.g., [Required], [MinLength])
            .ValidateDataAnnotations()
            // Ensures options are validated immediately when the app starts
            .ValidateOnStart();

        // Register the JWT provider as a singleton (one instance for entire application lifetime)
        services.AddSingleton<IJwtProvider, JwtProvider>();

        // Configure authentication services
        services.AddAuthentication(options =>
        {
            // Set the default authentication method (JWT Bearer)
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            // Set the default challenge (what happens if the user is not authenticated)
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        // Add JWT Bearer Authentication (this tells ASP.NET Core how to validate JWT tokens)
        .AddJwtBearer(options => {
            // SaveToken = true means the token will be stored inside AuthenticationProperties
            options.SaveToken = true;

            // Specify how the token should be validated
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Validate the signing key to ensure the token wasn't tampered with
                ValidateIssuerSigningKey = true,

                // Validate the Issuer (the server that created the token)
                ValidateIssuer = true,

                // Validate the Audience (who the token is intended for)
                ValidateAudience = true,

                // Validate token expiration (Reject expired tokens)
                ValidateLifetime = true,

                // Expected audience value from configuration
                ValidAudience = settings.Audience!,

                // Expected issuer value from configuration
                ValidIssuer = settings.Issuer!,

                // The secret key used to sign the JWT (must match the key used when generating the token)
                IssuerSigningKey = new SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(settings.Key!)
                )
            };
        });
        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 8;
            options.SignIn.RequireConfirmedEmail = false;
            options.User.RequireUniqueEmail = true;
        });

        return services;
    }
    private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Smart Parking Garage API",
                Version = "v1"
            });

            // 🔐 JWT Security Definition
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT token like: Bearer {your token}"
            });

            // 🔐 Apply JWT globally
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
        });

        return services;
    }
}
