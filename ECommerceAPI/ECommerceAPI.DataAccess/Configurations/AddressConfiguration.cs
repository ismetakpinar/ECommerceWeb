using ECommerceAPI.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAPI.DataAccess.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(x => x.AddressId);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Customer)
       .WithMany(x => x.Addresses)
       .HasForeignKey(x => x.CustomerId)
       .OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.Title)
    .IsRequired()
    .HasMaxLength(50);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.District)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FullAddress)
            .IsRequired()
            .HasMaxLength(500);
    }
}