using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using PersonalizedAssistant.Infrastructure.Configuration;
using PersonalizedAssistant.Infrastructure.Data;
using PersonalizedAssistant.Infrastructure.Services;
using PersonalizedAssistant.Shared.Interfaces;
using Serilog;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PersonalizedAssistant.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<AppSettings>(configuration);
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));
        var appSettings = configuration.Get<AppSettings>() ?? new AppSettings();

        // MongoDB
        services.AddSingleton<IMongoClient>(provider =>
        {
            var mongoSettings = provider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(mongoSettings.ConnectionString);
        });
        services.AddScoped<MongoDbContext>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDataCollectionService, DataCollectionService>();
        services.AddScoped<IDataProcessingService, DataProcessingService>();
        services.AddScoped<IChatAgentService, ChatAgentService>();

        // JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = appSettings.Jwt.Issuer,
                    ValidAudience = appSettings.Jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.SecretKey))
                };
            });

        // Authorization
        services.AddAuthorization();

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        // Health Checks
        services.AddHealthChecks()
            .AddMongoDb(appSettings.MongoDb.ConnectionString, name: "mongodb");

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Personalized Assistant API",
                Version = "v1",
                Description = "A personalized online assistant that processes user data from Google services and iOS to provide insights and recommendations."
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
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

    public static IServiceCollection AddLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var appSettings = configuration.Get<AppSettings>() ?? new AppSettings();
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.Console()
            .WriteTo.File($"{appSettings.Logging.LogPath}/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        services.AddSerilog();

        return services;
    }
}
