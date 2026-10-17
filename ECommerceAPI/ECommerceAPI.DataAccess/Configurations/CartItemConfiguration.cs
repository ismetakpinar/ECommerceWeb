using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAPI.DataAccess.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(x => x.CartItemId);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Cart)
       .WithMany(x => x.CartItems)
       .HasForeignKey(x => x.CartId)
       .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
       .WithMany(x => x.CartItems)
       .HasForeignKey(x => x.ProductId)
       .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.CartId,
            x.ProductId
        })
            .IsUnique().HasFilter("[IsDeleted] = 0");

    }
}