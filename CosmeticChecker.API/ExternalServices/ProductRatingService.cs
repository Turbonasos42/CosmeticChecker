using DatabaseContext;
using DatabaseModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class ProductRatingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductRatingService> _logger;

    public ProductRatingService(AppDbContext context, ILogger<ProductRatingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Метод для пересчета и установки SafetyRating для продуктов
    public async Task RecalculateAndSetSafetyRatingsAsync()
    {
        _logger.LogInformation("Начинаем пересчет SafetyRating для продуктов...");

        // Получаем все продукты с их связанными ингредиентами
        var products = await _context.Products
            .Include(p => p.ProductIngredients) // Включаем связь с ингредиентами
            .ThenInclude(pi => pi.Ingredient)   // Включаем сам ингредиент
            //.Take(10) // Берем только первые 10 продуктов для тестирования
            .ToListAsync();

        foreach (var product in products)
        {
            if (product.ProductIngredients.Any())
            {
                // Вычисляем средний рейтинг для ингредиентов этого продукта (SafetyRating)
                var averageRating = product.ProductIngredients
                    .Average(pi => pi.Ingredient.SafetyLevel); // Используем SafetyLevel ингредиента

                // Округляем до 2 знаков и сохраняем SafetyRating
                product.SafetyRating = Math.Round(averageRating, 2);
            }
            else
            {
                product.SafetyRating = 0; // Если у продукта нет ингредиентов, ставим рейтинг 0
            }
        }

        // Сохраняем изменения в базе данных
        await _context.SaveChangesAsync();

        _logger.LogInformation("Пересчет SafetyRating для продуктов завершен.");
    }


    // Метод для пересчета и установки UserRating для продуктов
    public async Task RecalculateAndSetUserRatingForProductAsync(int productId)
    {
        _logger.LogInformation($"Начинаем пересчет UserRating для продукта с ID {productId}...");

        // Получаем все одобренные отзывы для продукта с данным ID
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved) // Отбираем одобренные отзывы для конкретного продукта
            .ToListAsync();  // Загружаем все соответствующие отзывы

        if (reviews == null || !reviews.Any())
        {
            _logger.LogWarning($"Нет одобренных отзывов для продукта с ID {productId}.");
            return; // Если нет одобренных отзывов, выходим
        }

        // Вычисляем средний рейтинг для всех одобренных отзывов этого продукта
        var averageRating = reviews.Average(r => r.Rating);  // Средний рейтинг от всех одобренных отзывов

        // Получаем продукт по его ID, чтобы обновить UserRating
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
        {
            _logger.LogWarning($"Продукт с ID {productId} не найден.");
            return; // Если продукт не найден, выходим
        }

        // Устанавливаем средний рейтинг в поле UserRating (округляем до 2 знаков после запятой)
        product.UserRating = Math.Round(averageRating, 2);

        // Сохраняем изменения в базе данных
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Пересчет UserRating для продукта с ID {productId} завершен.");
    }


}
