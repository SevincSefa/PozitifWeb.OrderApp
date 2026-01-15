using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PozitifWeb.OrderApp.Domain.Entities;

namespace PozitifWeb.OrderApp.Infrastructure.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.Property(x => x.ProductName)
            .HasMaxLength(100)
            .IsRequired();

        b.Property(x => x.Quantity)
            .IsRequired();

        b.Property(x => x.UnitPrice)
            .IsRequired()
            .HasPrecision(15, 2);
        b.Property(x => x.CreatedDate).IsRequired();
        b.Property(x => x.UpdatedDate);

        b.HasIndex(x => x.OrderId);
    }
}