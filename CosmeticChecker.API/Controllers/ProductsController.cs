using CosmeticChecker.API.Services;
using DatabaseContext;
using DatabaseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CosmeticChecker.API.DTOs;

namespace Api.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // Расширенный поиск продуктов с фильтрами
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts(
            [FromQuery] string? name,          // Фильтр по названию
            [FromQuery] string? brand,         // Фильтр по бренду
            [FromQuery] string? category,      // Фильтр по категории
            [FromQuery] decimal? min_price,   // Минимальная цена
            [FromQuery] decimal? max_price,   // Максимальная цена
            [FromQuery] int? min_safety,      // Минимальный уровень безопасности (1-10)
            [FromQuery] int? max_safety,      // Максимальный уровень безопасности (1-10)
            [FromQuery] string?[] include_ingredients, // Обязательные ингредиенты
            [FromQuery] string?[] exclude_ingredients, // Исключаемые ингредиенты
            [FromQuery] string? skin_type,     // Тип кожи
            [FromQuery] int page = 1,          // Номер страницы
            [FromQuery] int pageSize = 10      // Размер страницы
        )
        {
            var products = await _productService.SearchProductsAsync(
                name, brand, category, min_price, max_price, min_safety, max_safety, include_ingredients, exclude_ingredients, skin_type, page, pageSize
            );

            return Ok(products);
        }

        // Получение детальной информации о продукте
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProductDetails(int id)
        {
            var product = await _productService.GetProductDetailsAsync(id);

            if (product == null)
                return NotFound();

            return Ok(product);
        }
    }

}
