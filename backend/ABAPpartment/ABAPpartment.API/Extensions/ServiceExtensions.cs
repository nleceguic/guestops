using ABAPpartment.Application.Interfaces;
using ABAPpartment.Application.Services;
using ABAPpartment.Domain.Interfaces;
using ABAPpartment.Infrastructure.AI;
using ABAPpartment.Infrastructure.Repositories;
using ABAPpartment.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Text;

namespace ABAPpartment.API.Extensions;

public static class ServiceExtensions
{
    /// <summary>Registra todos los servicios de la aplicación en el DI container.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IApartmentService, ApartmentService>();
        services.AddScoped<ICleaningScheduleService, CleaningScheduleService>();
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IGuestMessageService, GuestMessageService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IApartmentRepository, ApartmentRepository>();
        services.AddScoped<ICleaningScheduleRepository, CleaningScheduleRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IGuestMessageRepository, GuestMessageRepository>();
        services.AddScoped<IJwtService, JwtService>();

        // ── AI Assistant (Mock) ───────────────────────────────────────
        // Si quieres usar una API key de Anthropic:
        // 1. Cambia MockAIAssistantService por ClaudeAIAssistantService
        // 2. Añade en appsettings.json: "AnthropicApiKey": "sk-ant-..."
        services.AddScoped<IAIAssistantService, MockAIAssistantService>();

        return services;
    }

    /// <summary>Configura la autenticación JWT.</summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration config)
    {
        var secret = config["JwtSettings:Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret no configurado.");
        var issuer = config["JwtSettings:Issuer"] ?? "ABAPpartment.API";
        var audience = config["JwtSettings:Audience"] ?? "ABAPpartment.Client";

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                                                  Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    /// <summary>Configura Swagger con soporte para JWT Bearer.</summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "AB Apartment API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Introduce el token JWT. Ejemplo: eyJhbGci..."
            });

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