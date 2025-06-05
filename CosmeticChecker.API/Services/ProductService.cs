using CosmeticChecker.API.DTOs;
using Repositories;
using CosmeticChecker.API.DTOs;
using DatabaseModels;
using Repositories;

namespace CosmeticChecker.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ProductRatingService _productRatingService;


        public ProductService(IProductRepository productRepository, ProductRatingService productRatingService)
        {
            _productRepository = productRepository;
            _productRatingService = productRatingService;

        }

        public async Task<ProductListDto> SearchProductsAsync(
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
     int pageSize)
        {
            // Получаем все продукты с помощью нового метода
            var products = await _productRepository.GetAllProductsAsync();

            // Применяем фильтры, используя LINQ
            if (!string.IsNullOrEmpty(name))
            {
                products = products.Where(p => p.Name.Contains(name)).ToList();
            }

            if (!string.IsNullOrEmpty(brand))
            {
                products = products.Where(p => p.Brand.Contains(brand)).ToList();
            }

            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category.Contains(category)).ToList();
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value).ToList();
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value).ToList();
            }

            if (minSafety.HasValue)
            {
                products = products.Where(p => p.SafetyRating >= minSafety.Value).ToList();
            }

            if (maxSafety.HasValue)
            {
                products = products.Where(p => p.SafetyRating <= maxSafety.Value).ToList();
            }

            if (includeIngredients != null && includeIngredients.Length > 0)
            {
                foreach (var ingredient in includeIngredients)
                {
                    products = products.Where(p => p.Ingredients.Contains(ingredient)).ToList();
                }
            }

            if (excludeIngredients != null && excludeIngredients.Length > 0)
            {
                foreach (var ingredient in excludeIngredients)
                {
                    products = products.Where(p => !p.Ingredients.Contains(ingredient)).ToList();
                }
            }

            if (!string.IsNullOrEmpty(skinType))
            {
                products = products.Where(p => p.SkinType.Contains(skinType)).ToList();
            }

            // Сортировка по Id, если фильтры не были применены
            products = products.OrderBy(p => p.Id).ToList();

            // Получаем общее количество продуктов
            var totalProducts = products.Count;

            // Вычисляем количество страниц
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            // Применяем пагинацию
            var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Преобразуем в DTO
            var productDtos = pagedProducts.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                Brand = p.Brand,
                Price = p.Price,
                SafetyRating = p.SafetyRating
            }).ToList();

            // Возвращаем результат
            return new ProductListDto
            {
                Products = productDtos,
                TotalCount = totalProducts
            };
        }



        public async Task<ProductDto> GetProductDetailsAsync(int productId)
        {
            var product = await _productRepository.GetProductByIdAsync(productId);
            await _productRatingService.RecalculateAndSetUserRatingForProductAsync(productId);


            if (product == null)
                return null;

            return new ProductDto
            {
                
                Id = product.Id,
                Name = product.Name,
                Brand = product.Brand,
                Price = product.Price,
                Category = product.Category,
                ProductType = product.ProductType,
                SkinType = product.SkinType,
                Ingredients = product.Ingredients,
                SafetyRating = product.SafetyRating,
                UserRatings = product.UserRating,
                Description = product.Description,
                Analogues = product.Analogues
            };
        }
    }

}
