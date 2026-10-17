using ECommerceAPI.DataAccess.Context;
using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceAPI.DataAccess.Initializers;

public static class DatabaseInitializer
{
    private static readonly Dictionary<string, (string Name, string Category)> ProductImageMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["product-1.jpg"] = ("DSLR Kamera", "Elektronik"),
        ["product-2.jpg"] = ("Mavi Sweatshirt", "Giyim"),
        ["product-3.jpg"] = ("Masa Lambası", "Ev & Yaşam"),
        ["product-4.jpg"] = ("Çocuk Spor Ayakkabısı", "Ayakkabı"),
        ["product-5.jpg"] = ("Kameralı Drone", "Elektronik"),
        ["product-6.jpg"] = ("Akıllı Saat", "Elektronik"),
        ["product-7.jpg"] = ("Dantel Detaylı Bluz", "Giyim"),
        ["product-8.jpg"] = ("Cilt Bakım Seti", "Kozmetik"),
        ["product-9.jpg"] = ("Ofis Koltuğu", "Mobilya")
    };

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ECommerceDbContext>();

        await InitializeAsync(context);
    }

    public static async Task InitializeAsync(ECommerceDbContext context)
    {
        await context.Database.MigrateAsync();

        await SeedCategoriesAsync(context);
        await SeedProductsAsync(context);
        await SeedAdminUserAsync(context);

    }

    private static async Task SeedCategoriesAsync(ECommerceDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new()
            {
                Name = "Elektronik"
            },
            new()
            {
                Name = "Giyim"
            },
            new()
            {
                Name = "Ev & Yaşam"
            }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ECommerceDbContext context)
    {
        // If products exist, ensure their ImageUrl fields are set to match frontend assets
        if (await context.Products.AnyAsync())
        {
            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Laptop"] = "/theme/img/product-1.jpg",
                ["Tişört"] = "/theme/img/product-2.jpg",
                ["Masa Lambası"] = "/theme/img/product-3.jpg",
                ["Suç ve Ceza"] = "https://images.unsplash.com/photo-1544947950-fa07a98d237f?auto=format&fit=crop&w=600&h=600&q=85"
            };

            var existing = await context.Products.ToListAsync();
            var updated = false;

            foreach (var p in existing)
            {
                if (string.IsNullOrWhiteSpace(p.ImageUrl) && mapping.TryGetValue(p.Name, out var path))
                {
                    p.ImageUrl = path;
                    updated = true;
                }
            }

            if (updated)
                await context.SaveChangesAsync();

            // Also attempt to add products based on frontend images if there are missing ones
        }

        // Ensure required categories exist; create them if missing
        var elektronik = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Elektronik");
        var giyim = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Giyim");
        var evYasam = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Ev & Yaşam");

        var categoriesToAdd = new List<Category>();
        if (elektronik is null)
        {
            elektronik = new Category { Name = "Elektronik" };
            categoriesToAdd.Add(elektronik);
        }

        if (giyim is null)
        {
            giyim = new Category { Name = "Giyim" };
            categoriesToAdd.Add(giyim);
        }

        if (evYasam is null)
        {
            evYasam = new Category { Name = "Ev & Yaşam" };
            categoriesToAdd.Add(evYasam);
        }

        if (categoriesToAdd.Any())
        {
            await context.Categories.AddRangeAsync(categoriesToAdd);
            await context.SaveChangesAsync();
        }

        // Soft-delete legacy carousel placeholder products (names containing 'Carousel')
        var carouselPlaceholders = await context.Products
            .Where(p => !p.IsDeleted && p.Name != null && (p.Name.ToLower().StartsWith("carousel") || p.Name.ToLower().Contains("carousel")))
            .ToListAsync();
        if (carouselPlaceholders.Any())
        {
            foreach (var cprod in carouselPlaceholders)
            {
                cprod.IsDeleted = true;
                cprod.DeletedDate = DateTime.UtcNow;
            }
            await context.SaveChangesAsync();
        }

        // Ensure any previously soft-deleted product entries for these images are restored later during processing.
        // (No preemptive deletion here — creation/restore is handled per-image below)

        // Discover frontend image assets to seed products automatically
        var candidateDirs = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "ECommerce.Web", "ECommerce.Web", "wwwroot", "theme", "img"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "ECommerce.Web", "ECommerce.Web", "wwwroot", "theme", "img"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "ECommerce.Web", "ECommerce.Web", "wwwroot", "theme", "img")
        };

        string? imagesDir = candidateDirs.FirstOrDefault(Directory.Exists);

        var imageFiles = new List<string>();
        if (!string.IsNullOrEmpty(imagesDir))
        {
            // include all common image extensions in the folder (add any image found)
            var patterns = new[] { "product-*.jpg", "product-*.png", "product-*.jpeg", "product-*.webp" };
            foreach (var p in patterns)
                imageFiles.AddRange(Directory.GetFiles(imagesDir, p, SearchOption.TopDirectoryOnly));

            imageFiles = imageFiles.Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(f => ProductImageMappings.ContainsKey(Path.GetFileName(f)))
                .OrderBy(f => Path.GetFileName(f))
                .ToList();
        }

        var productsToAdd = new List<Product>();

        if (imageFiles.Any())
        {
            // mapping filename -> (Name, CategoryName)
            // Ensure mapped categories exist
            var categoriesNeeded = ProductImageMappings.Values.Select(v => v.Category).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var existingCategories = await context.Categories.ToListAsync();
            var categoriesToCreate = new List<Category>();
            foreach (var catName in categoriesNeeded)
            {
                if (!existingCategories.Any(c => c.Name.Equals(catName, StringComparison.OrdinalIgnoreCase)))
                {
                    categoriesToCreate.Add(new Category { Name = catName });
                }
            }
            if (categoriesToCreate.Any())
            {
                await context.Categories.AddRangeAsync(categoriesToCreate);
                await context.SaveChangesAsync();
                existingCategories.AddRange(categoriesToCreate);
            }

            var existingNames = await context.Products.Select(p => p.Name).ToListAsync();

            var idx = 1;
            foreach (var file in imageFiles.OrderBy(x => x))
            {
                var fileName = Path.GetFileName(file);
                var imageUrl = $"/theme/img/{fileName}";

                // Determine friendly name and category from mapping, fallback to filename-based name
                string friendlyName;
                string categoryName;
                if (ProductImageMappings.TryGetValue(fileName, out var map))
                {
                    friendlyName = map.Name;
                    categoryName = map.Category;
                }
                else
                {
                    friendlyName = Path.GetFileNameWithoutExtension(fileName).Replace('-', ' ').Replace('_', ' ');
                    friendlyName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(friendlyName);
                    categoryName = giyim.Name; // default
                }

                // If a product with this image already exists, update its name/category if needed
                var existingProduct = await context.Products.FirstOrDefaultAsync(p => p.ImageUrl == imageUrl);
                var categoryEntity = existingCategories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                var categoryId = categoryEntity?.CategoryId ?? giyim.CategoryId;

                if (existingProduct != null)
                {
                    var updated = false;

                    // If product was soft-deleted, restore it
                    if (existingProduct.IsDeleted)
                    {
                        existingProduct.IsDeleted = false;
                        existingProduct.DeletedDate = null;
                        updated = true;
                    }

                    if (!existingProduct.Name.Equals(friendlyName, StringComparison.OrdinalIgnoreCase))
                    {
                        existingProduct.Name = friendlyName;
                        updated = true;
                    }
                    if (existingProduct.CategoryId != categoryId)
                    {
                        existingProduct.CategoryId = categoryId;
                        updated = true;
                    }

                    // Ensure image url and featured flag
                    if (!string.Equals(existingProduct.ImageUrl, imageUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        existingProduct.ImageUrl = imageUrl;
                        updated = true;
                    }

                    if (existingProduct.IsFeatured)
                    {
                        existingProduct.IsFeatured = false; // remove from carousel
                        updated = true;
                    }

                    if (updated)
                    {
                        await context.SaveChangesAsync();
                    }

                    continue;
                }

                if (existingNames.Contains(friendlyName, StringComparer.OrdinalIgnoreCase))
                    friendlyName += $" {idx}"; // make unique

                var p = new Product
                {
                    Name = friendlyName,
                    Description = string.Empty,
                    Price = 199.99m + (decimal)(idx * 10),
                    Stock = 20,
                    CategoryId = categoryId,
                    ImageUrl = imageUrl,
                    IsFeatured = false
                };

                productsToAdd.Add(p);
                idx++;
            }

            if (productsToAdd.Any())
            {
                await context.Products.AddRangeAsync(productsToAdd);
                await context.SaveChangesAsync();
            }

            // Normalize categories for all non-deleted products based on image mapping and name heuristics
            var allProducts = await context.Products.Where(p => !p.IsDeleted).ToListAsync();
            var categoryLookup = existingCategories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
            var categoriesAdded = new List<Category>();
            foreach (var prod in allProducts)
            {
                string? desiredCategoryName = null;
                if (!string.IsNullOrEmpty(prod.ImageUrl))
                {
                    var fileName = Path.GetFileName(prod.ImageUrl);
                    if (ProductImageMappings.TryGetValue(fileName, out var map2))
                        desiredCategoryName = map2.Category;
                }

                if (desiredCategoryName is null)
                {
                    var n = (prod.Name ?? string.Empty).ToLowerInvariant();
                    if (n.Contains("sweat") || n.Contains("tişört") || n.Contains("blouse") || n.Contains("bluz") || n.Contains("giyim"))
                        desiredCategoryName = "Giyim";
                    else if (n.Contains("sneaker") || n.Contains("ayakkabı"))
                        desiredCategoryName = "Ayakkabı";
                    else if (n.Contains("lamp") || n.Contains("table") || n.Contains("lamba") || n.Contains("dekor"))
                        desiredCategoryName = "Ev & Yaşam";
                    else if (n.Contains("chair") || n.Contains("koltuk") || n.Contains("mobilya"))
                        desiredCategoryName = "Mobilya";
                    else if (n.Contains("camera") || n.Contains("foto") || n.Contains("lens") || n.Contains("drone") || n.Contains("watch") || n.Contains("smart") || n.Contains("iphone"))
                        desiredCategoryName = "Elektronik";
                    else if (n.Contains("skincare") || n.Contains("curology") || n.Contains("cream") || n.Contains("kozmetik") || n.Contains("makyaj"))
                        desiredCategoryName = "Kozmetik";
                    else if (n.Contains("kitap") || n.Contains("book"))
                        desiredCategoryName = "Kitap";
                }

                if (desiredCategoryName != null)
                {
                    if (!categoryLookup.TryGetValue(desiredCategoryName, out var cat))
                    {
                        cat = new Category { Name = desiredCategoryName };
                        categoriesAdded.Add(cat);
                        categoryLookup[desiredCategoryName] = cat;
                    }

                    // If category is newly created, ensure it gets an ID by saving later
                    if (cat.CategoryId == 0)
                    {
                        // postpone assigning until after SaveChanges when ID exists
                    }
                }
            }

            if (categoriesAdded.Any())
            {
                await context.Categories.AddRangeAsync(categoriesAdded);
                await context.SaveChangesAsync();
                // refresh lookup with IDs
                existingCategories = await context.Categories.ToListAsync();
                categoryLookup = existingCategories.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
            }

            var changed = false;
            foreach (var prod in allProducts)
            {
                string? desiredCategoryName = null;
                if (!string.IsNullOrEmpty(prod.ImageUrl))
                {
                    var fileName = Path.GetFileName(prod.ImageUrl);
                    if (ProductImageMappings.TryGetValue(fileName, out var map2))
                        desiredCategoryName = map2.Category;
                }

                if (desiredCategoryName is null)
                {
                    var n = (prod.Name ?? string.Empty).ToLowerInvariant();
                    if (n.Contains("sweat") || n.Contains("tişört") || n.Contains("blouse") || n.Contains("bluz") || n.Contains("giyim"))
                        desiredCategoryName = "Giyim";
                    else if (n.Contains("sneaker") || n.Contains("ayakkabı"))
                        desiredCategoryName = "Ayakkabı";
                    else if (n.Contains("lamp") || n.Contains("table") || n.Contains("lamba") || n.Contains("dekor"))
                        desiredCategoryName = "Ev & Yaşam";
                    else if (n.Contains("chair") || n.Contains("koltuk") || n.Contains("mobilya"))
                        desiredCategoryName = "Mobilya";
                    else if (n.Contains("camera") || n.Contains("foto") || n.Contains("lens") || n.Contains("drone") || n.Contains("watch") || n.Contains("smart") || n.Contains("iphone"))
                        desiredCategoryName = "Elektronik";
                    else if (n.Contains("skincare") || n.Contains("curology") || n.Contains("cream") || n.Contains("kozmetik") || n.Contains("makyaj"))
                        desiredCategoryName = "Kozmetik";
                    else if (n.Contains("kitap") || n.Contains("book"))
                        desiredCategoryName = "Kitap";
                }

                if (desiredCategoryName != null && categoryLookup.TryGetValue(desiredCategoryName, out var desiredCat))
                {
                    if (prod.CategoryId != desiredCat.CategoryId)
                    {
                        prod.CategoryId = desiredCat.CategoryId;
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                await context.SaveChangesAsync();
            }

            var hardwareCategory = await context.Categories
                .FirstOrDefaultAsync(c => c.Name == "Hırdavat");

            if (hardwareCategory is null)
            {
                hardwareCategory = new Category { Name = "Hırdavat" };
                await context.Categories.AddAsync(hardwareCategory);
                await context.SaveChangesAsync();
            }

            if (!await context.Products.AnyAsync(p =>
                !p.IsDeleted && p.CategoryId == hardwareCategory.CategoryId))
            {
                await context.Products.AddAsync(new Product
                {
                    Name = "Akülü Matkap Seti",
                    Description = "Ev ve atölye projeleri için güçlü akülü matkap seti.",
                    Price = 2499.99m,
                    Stock = 15,
                    CategoryId = hardwareCategory.CategoryId,
                    ImageUrl = "https://images.unsplash.com/photo-1504148455328-c376907d081c?auto=format&fit=crop&w=600&h=600&q=85",
                    IsFeatured = false
                });
                await context.SaveChangesAsync();
            }

            // Remove categories that aren't used by any non-deleted product
            var usedCategoryIds = await context.Products
                .Where(p => !p.IsDeleted)
                .Select(p => p.CategoryId)
                .Distinct()
                .ToListAsync();

            var categoriesToDelete = await context.Categories
                .Where(c => !usedCategoryIds.Contains(c.CategoryId))
                .ToListAsync();

            if (categoriesToDelete.Any())
            {
                context.Categories.RemoveRange(categoriesToDelete);
                await context.SaveChangesAsync();
            }

            return;
        }

        // Fallback seed if no images discovered
        var products = new List<Product>
        {
            new()
            {
                Name = "DSLR Kamera",
                Description = "Fotoğrafçılık için dijital SLR kamera.",
                Price = 35000m,
                Stock = 10,
                CategoryId = elektronik.CategoryId,
                ImageUrl = "/theme/img/product-1.jpg",
                IsFeatured = true
            },
            new()
            {
                Name = "Mavi Sweatshirt",
                Description = "Günlük kullanım için mavi sweatshirt.",
                Price = 500m,
                Stock = 100,
                CategoryId = giyim.CategoryId,
                ImageUrl = "/theme/img/product-2.jpg",
                IsFeatured = true
            },
            new()
            {
                Name = "Masa Lambası",
                Description = "Yaşam alanları için dekoratif masa lambası.",
                Price = 850m,
                Stock = 20,
                CategoryId = evYasam.CategoryId,
                ImageUrl = "/theme/img/product-3.jpg",
                IsFeatured = false
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
    private static async Task SeedAdminUserAsync(ECommerceDbContext context)
    {
        if (await context.AppUsers.AnyAsync())
            return;

        var admin = new AppUser
        {
            Email = "testcustomer@mail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            Role = "Admin"
        };

        var customer = new Customer
        {
            FirstName = "Test",
            LastName = "Customer",
            Phone = "05555555555",
            AppUser = admin
        };

        admin.Customer = customer;

        await context.AppUsers.AddAsync(admin);

        await context.SaveChangesAsync();
    }

    private static async Task SeedOrdersAsync(ECommerceDbContext context)
    {
        if (await context.Orders.AnyAsync())
            return;

        var customer = await context.Customers.FirstAsync();
        var product = await context.Products.FirstAsync();

        var order = new Order
        {
            CustomerId = customer.CustomerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            ShippingAddressTitle = "Ev",
            ShippingCity = "İstanbul",
            ShippingDistrict = "Kadıköy",
            ShippingFullAddress = "Test Mahallesi No:1",
            TotalAmount = product.Price
        };

        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        var orderDetail = new OrderDetail
        {
            OrderId = order.OrderId,
            ProductId = product.ProductId,
            Quantity = 1,
            UnitPrice = product.Price,
            ProductName = product.Name
        };

        await context.OrderDetails.AddAsync(orderDetail);
        await context.SaveChangesAsync();
    }

}