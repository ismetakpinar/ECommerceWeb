using ECommerceAPI.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAPI.DataAccess.Configurations;

public class OrderDetailConfiguration
    : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.HasKey(x => x.OrderDetailId);

        builder.Property(x => x.UnitPrice)
               .HasPrecision(18, 2);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Order)
       .WithMany(x => x.OrderDetails)
       .HasForeignKey(x => x.OrderId)
       .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
       .WithMany(x => x.OrderDetails)
       .HasForeignKey(x => x.ProductId)
       .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ProductName)
            .IsRequired()
            .HasMaxLength(100);
    }
}