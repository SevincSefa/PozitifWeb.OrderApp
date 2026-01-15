using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PozitifWeb.OrderApp.Domain.Entities;

namespace PozitifWeb.OrderApp.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.Property(x => x.OrderDate)
            .IsRequired();

        b.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        b.Property(x => x.TotalAmount)
            .IsRequired()
            .HasPrecision(15, 2);

        b.Property(x => x.CreatedDate).IsRequired();
        b.Property(x => x.UpdatedDate);

        // 5 gün kuralı için
        b.HasIndex(x => new { x.CustomerId, x.OrderDate });

        b.HasMany(x => x.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}