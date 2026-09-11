using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderFlow.Infrastructure.Orders.DataSource.ModelConfig;

internal class KnownSkuConfig : IEntityTypeConfiguration<KnownSku>
{
    const int MaximunLengthSku = 50;

    public void Configure(EntityTypeBuilder<KnownSku> builder)
    {
        builder.ToTable("KnownSkus");

        builder.Property(knownSku => knownSku.Id)
            .IsRequired();

        builder.Property(knownSku => knownSku.Sku)
            .HasMaxLength(MaximunLengthSku)
            .IsRequired();

        builder.HasIndex(knownSku => knownSku.Sku)
            .IsUnique();
    }
}
