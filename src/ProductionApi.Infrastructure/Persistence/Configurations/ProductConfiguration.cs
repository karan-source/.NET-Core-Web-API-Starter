using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductionApi.Domain.Entities;

namespace ProductionApi.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(product => product.Description)
            .HasMaxLength(2000);

        builder.Property(product => product.Price)
            .HasPrecision(18, 2);

        // Indexes match the filter and sort used by the Dapper paging query.
        builder.HasIndex(product => product.Name);
        builder.HasIndex(product => product.CreatedAtUtc);
    }
}
