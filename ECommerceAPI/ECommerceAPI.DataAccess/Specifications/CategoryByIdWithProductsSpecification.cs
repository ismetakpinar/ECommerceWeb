using ECommerceAPI.Entity.Entities;

namespace ECommerceAPI.DataAccess.Specifications;

public class CategoryByIdWithProductsSpecification
    : BaseSpecification<Category>
{
    public CategoryByIdWithProductsSpecification(int categoryId)
        : base(x => x.CategoryId == categoryId)
    {
        AddInclude(x => x.Products);
    }
}