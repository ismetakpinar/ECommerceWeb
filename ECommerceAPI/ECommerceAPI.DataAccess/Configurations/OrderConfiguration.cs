using ECommerceAPI.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAPI.DataAccess.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.OrderId);
        builder.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Customer)
       .WithMany(x => x.Orders)
       .HasForeignKey(x => x.CustomerId)
       .OnDelete(DeleteBehavior.Restrict);
    }
}