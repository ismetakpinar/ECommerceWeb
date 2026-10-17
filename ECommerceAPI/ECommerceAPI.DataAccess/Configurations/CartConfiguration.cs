using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAPI.DataAccess.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(x => x.CartId);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Customer)
       .WithOne(x => x.Cart)
       .HasForeignKey<Cart>(x => x.CustomerId)
       .OnDelete(DeleteBehavior.Cascade);
    }
}