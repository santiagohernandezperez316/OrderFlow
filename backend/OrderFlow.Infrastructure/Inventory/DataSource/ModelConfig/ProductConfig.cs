using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Inventory.Entity;

namespace OrderFlow.Infrastructure.Inventory.DataSource.ModelConfig;

internal class ProductConfig : IEntityTypeConfiguration<Product>
{
    const int MaximunLengthSku = 50;
    const int MaximunLengthName = 100;

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(product => product.Id)
            .IsRequired();

        builder.Property(product => product.Sku)
            .HasMaxLength(MaximunLengthSku)
            .IsRequired();

        builder.Property(product => product.Name)
            .HasMaxLength(MaximunLengthName)
            .IsRequired();

        builder.HasIndex(product => product.Sku)
            .IsUnique();
    }
}
