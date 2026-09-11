using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Orders.Entity;

namespace OrderFlow.Infrastructure.Orders.DataSource.ModelConfig;

internal class OrderConfig : IEntityTypeConfiguration<Order>
{
    const int MaximunLengthClienteNombre = 200;
    const int MaximunLengthSku = 50;

    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(order => order.Id)
            .IsRequired();

        builder.Property(order => order.ClienteNombre)
            .HasMaxLength(MaximunLengthClienteNombre)
            .IsRequired();

        builder.Property(order => order.Sku)
            .HasMaxLength(MaximunLengthSku)
            .IsRequired();

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .IsRequired();
    }
}
