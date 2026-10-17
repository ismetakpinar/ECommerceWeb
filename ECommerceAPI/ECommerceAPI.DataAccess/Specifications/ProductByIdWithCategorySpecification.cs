using ECommerceAPI.Entity.Entities;

namespace ECommerceAPI.DataAccess.Specifications;

public class ProductByIdWithCategorySpecification
    : BaseSpecification<Product>
{
    public ProductByIdWithCategorySpecification(int productId)
        : base(x => x.ProductId == productId)
    {
        AddInclude(x => x.Category);
    }
}