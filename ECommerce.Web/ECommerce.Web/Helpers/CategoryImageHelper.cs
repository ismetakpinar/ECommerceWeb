namespace ECommerce.Web.Helpers;

public static class CategoryImageHelper
{
    private static readonly IReadOnlyDictionary<string, string> CarouselImagePaths =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Elektronik"] = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=2400&h=1000&q=90",
            ["Giyim"] = "https://images.unsplash.com/photo-1483985988355-763728e1935b?auto=format&fit=crop&w=2400&h=1000&q=90",
            ["Ayakkabı"] = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=2400&h=1000&q=90",
            ["Kozmetik"] = "https://images.unsplash.com/photo-1596462502278-27bfdc403348?auto=format&fit=crop&w=2400&h=1000&q=90",
            ["Kitap"] = "https://images.unsplash.com/photo-1507842217343-583bb7270b66?auto=format&fit=crop&w=2400&h=1000&q=90",
            ["Ev & Yaşam"] = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?auto=format&fit=crop&w=2400&h=1000&q=90",
            ["Ev & Dekor"] = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?auto=format&fit=crop&w=2400&h=1000&q=90",
            ["Mobilya"] = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?auto=format&fit=crop&w=2400&h=1000&q=90",
            ["Hırdavat"] = "https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?auto=format&fit=crop&w=2400&h=1000&q=90",
            ["Hirdavat"] = "https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?auto=format&fit=crop&w=2400&h=1000&q=90"
        };

    private static readonly IReadOnlyDictionary<string, string> ImagePaths =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Elektronik"] = "~/theme/img/cat-2.jpg",
            ["Giyim"] = "~/theme/img/cat-1.jpg",
            ["Ayakkabı"] = "~/theme/img/cat-3.jpg",
            ["Kozmetik"] = "~/theme/img/cat-4.jpg",
            ["Kitap"] = "https://images.unsplash.com/photo-1544947950-fa07a98d237f?auto=format&fit=crop&w=600&h=600&q=85",
            ["Ev & Yaşam"] = "~/theme/img/product-3.jpg",
            ["Ev & Dekor"] = "~/theme/img/product-3.jpg",
            ["Mobilya"] = "~/theme/img/product-9.jpg",
            ["Hırdavat"] = "https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?auto=format&fit=crop&w=600&h=600&q=85",
            ["Hirdavat"] = "https://images.unsplash.com/photo-1586864387967-d02ef85d93e8?auto=format&fit=crop&w=600&h=600&q=85"
        };

    public static string? GetImagePath(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return null;
        }

        return ImagePaths.TryGetValue(categoryName.Trim(), out var imagePath)
            ? imagePath
            : null;
    }

    public static string? GetCarouselImagePath(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return null;
        }

        return CarouselImagePaths.TryGetValue(categoryName.Trim(), out var imagePath)
            ? imagePath
            : null;
    }
}