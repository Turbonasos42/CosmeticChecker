using CosmeticChecker.API.DTOs; 

namespace CosmeticChecker.API.Services
{
    public interface IProductService
    {
        Task<ProductListDto> SearchProductsAsync(
            string name,
            string brand,
            string category,
            decimal? minPrice,
            decimal? maxPrice,
            int? minSafety,
            int? maxSafety,
            string[] includeIngredients,
            string[] excludeIngredients,
            string skinType,
            int page,
            int pageSize);

        Task<ProductDto> GetProductDetailsAsync(int productId);
    }


}
