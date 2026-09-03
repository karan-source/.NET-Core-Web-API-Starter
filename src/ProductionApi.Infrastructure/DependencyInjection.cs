using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductionApi.Application.Common.Interfaces;
using ProductionApi.Infrastructure.Identity;
using ProductionApi.Infrastructure.Persistence;
using ProductionApi.Infrastructure.Persistence.Queries;
using ProductionApi.Infrastructure.Services;

namespace ProductionApi.Infrastructure;

public static class DependencyInjection
{
    private const int MinimumSigningKeyBytes = 32;

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ApplicationDbContextInitialiser>();

        SqliteTypeHandlers.Register();
        services.AddSingleton<ISqlConnectionFactory>(_ => new SqliteConnectionFactory(connectionString));
        services.AddScoped<IProductReadRepository, ProductReadRepository>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton(ReadJwtOptions(configuration));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }

    public static JwtOptions ReadJwtOptions(IConfiguration configuration)
    {
        var options = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException($"Configuration section '{JwtOptions.SectionName}' is missing.");

        // Fail fast at startup rather than issuing tokens signed with a weak or absent key.
        if (Encoding.UTF8.GetByteCount(options.SigningKey) < MinimumSigningKeyBytes)
        {
            throw new InvalidOperationException(
                $"'{JwtOptions.SectionName}:SigningKey' must be at least {MinimumSigningKeyBytes} bytes. " +
                "Supply it via user secrets or an environment variable - never commit it.");
        }

        if (string.IsNullOrWhiteSpace(options.Issuer) || string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException($"'{JwtOptions.SectionName}:Issuer' and 'Audience' must be configured.");
        }

        return options;
    }
}
