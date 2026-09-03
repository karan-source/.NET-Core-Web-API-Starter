using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductionApi.Application.Common.Interfaces;
using ProductionApi.Domain.Entities;

namespace ProductionApi.Infrastructure.Persistence;

public sealed class ApplicationDbContextInitialiser(
    ApplicationDbContext context,
    IDateTimeProvider clock,
    ILogger<ApplicationDbContextInitialiser> logger)
{
    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (context.Database.GetMigrations().Any())
            {
                await context.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                await context.Database.EnsureCreatedAsync(cancellationToken);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Database initialisation failed.");
            throw;
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await context.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = clock.UtcNow;

        context.Products.AddRange(
            new Product { Name = "Mechanical Keyboard", Description = "Hot-swappable, 87 keys.", Price = 129.99m, StockQuantity = 42, CreatedAtUtc = now },
            new Product { Name = "27\" 4K Monitor", Description = "IPS panel, USB-C power delivery.", Price = 449.00m, StockQuantity = 15, CreatedAtUtc = now },
            new Product { Name = "Noise Cancelling Headphones", Description = "Over-ear, 30 hour battery.", Price = 249.50m, StockQuantity = 0, IsActive = false, CreatedAtUtc = now },
            new Product { Name = "Standing Desk", Description = "Electric, dual motor.", Price = 599.00m, StockQuantity = 7, CreatedAtUtc = now });

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded initial product catalogue.");
    }
}
