using ECommerceAPI.Entity.Enums;
namespace ECommerceAPI.Dto.Requests;

public sealed class ProductQueryRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int? CategoryId { get; set; }

    public bool? IsFeatured { get; set; }

    public string? Search { get; set; }

    public ProductSortBy SortBy { get; set; } = ProductSortBy.None;
}