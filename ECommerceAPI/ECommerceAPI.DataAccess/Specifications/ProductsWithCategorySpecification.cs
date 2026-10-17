using ECommerceAPI.Entity.Entities;
using ECommerceAPI.Entity.Enums;
namespace ECommerceAPI.DataAccess.Specifications;

public sealed class ProductsWithCategorySpecification
    : BaseSpecification<Product>
{
    public ProductsWithCategorySpecification(
        int page,
        int pageSize,
        int? categoryId,
        bool? isFeatured,
        string? search,
        ProductSortBy sortBy)
        : base(product =>

            (!categoryId.HasValue ||
             product.CategoryId == categoryId.Value)

            &&

            (!isFeatured.HasValue ||
             product.IsFeatured == isFeatured.Value)

            &&

            (string.IsNullOrWhiteSpace(search) ||
             product.Name.Contains(search)))
    {
        AddInclude(product => product.Category);

        switch (sortBy)
        {
            case ProductSortBy.NameAsc:
                ApplyOrderBy(product => product.Name);
                break;

            case ProductSortBy.NameDesc:
                ApplyOrderByDescending(product => product.Name);
                break;

            case ProductSortBy.PriceAsc:
                ApplyOrderBy(product => product.Price);
                break;

            case ProductSortBy.PriceDesc:
                ApplyOrderByDescending(product => product.Price);
                break;

            default:
                ApplyOrderByDescending(product => product.ProductId);
                break;
        }

        ApplyPaging(
            (page - 1) * pageSize,
            pageSize);
    }
}