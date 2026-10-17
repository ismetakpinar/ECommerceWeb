using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAPI.DataAccess.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(x => x.CustomerId);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.AppUser)
       .WithOne(x => x.Customer)
       .HasForeignKey<Customer>(x => x.AppUserId)
       .OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(20);
    }
}