using ECommerceAPI.Entity.Common;
using ECommerceAPI.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.DataAccess.Context;

public class ECommerceDbContext : DbContext
{
    public ECommerceDbContext(
        DbContextOptions<ECommerceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ECommerceDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var currentDate = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = currentDate;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedDate = currentDate;

                entry.Property(x => x.CreatedDate)
                    .IsModified = false;

                var wasDeleted = entry
                    .OriginalValues
                    .GetValue<bool>(nameof(BaseEntity.IsDeleted));

                if (!wasDeleted && entry.Entity.IsDeleted)
                {
                    entry.Entity.DeletedDate = currentDate;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}