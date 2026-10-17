using ECommerceAPI.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAPI.DataAccess.Configurations;

public class CategoryConfiguration
    : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(x => x.CategoryId);

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasData(
            new Category
            {
                CategoryId = 1,
                Name = "Elektronik"
            },
            new Category
            {
                CategoryId = 2,
                Name = "Giyim"
            },
            new Category
            {
                CategoryId = 3,
                Name = "Kitap"
            },
            new Category
            {
                CategoryId = 4,
                Name = "Ev & Yaşam"
            });
        builder.Property(x => x.Name)
    .IsRequired()
    .HasMaxLength(100);
    }

}