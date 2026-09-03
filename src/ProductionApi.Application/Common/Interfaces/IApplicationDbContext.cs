using Microsoft.EntityFrameworkCore;
using ProductionApi.Domain.Entities;

namespace ProductionApi.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
