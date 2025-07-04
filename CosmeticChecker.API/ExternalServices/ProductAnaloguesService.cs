using Microsoft.EntityFrameworkCore;
using DatabaseContext;
using DatabaseModels;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;



namespace CosmeticChecker.API.ExternalServices
{
    public class ProductAnaloguesService : IProductAnaloguesService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductAnaloguesService> _logger;

        public ProductAnaloguesService(AppDbContext context, ILogger<ProductAnaloguesService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task FindAndSetProductAnaloguesAsync()
        {
            _logger.LogInformation("Начинаем поиск аналогов для продуктов...");

            // Получаем все продукты с их ингредиентами
            var products = await _context.Products
                .Include(p => p.ProductIngredients)  // Включаем связи с ингредиентами
                .ThenInclude(pi => pi.Ingredient)   // Включаем сами ингредиенты
                .ToListAsync();

            foreach (var product in products)
            {
                var productIngredients = product.ProductIngredients.Select(pi => pi.Ingredient.Name).ToList();

                var analogues = new List<string>();

                foreach (var otherProduct in products)
                {
                    if (product.Id == otherProduct.Id) continue; // Пропускаем сам продукт

                    var otherProductIngredients = otherProduct.ProductIngredients.Select(pi => pi.Ingredient.Name).ToList();

                    // Считаем количество совпадающих ингредиентов
                    var commonIngredients = productIngredients.Intersect(otherProductIngredients).Count();
                    var totalIngredients = productIngredients.Count;

                    // Если совпадение более 70%, добавляем продукт в список аналогов
                    if ((double)commonIngredients / totalIngredients >= 0.7)
                    {
                        analogues.Add(otherProduct.Name);
                    }
                }

                // Записываем аналогичные продукты в поле Analogues
                product.Analogues = string.Join(", ", analogues);

                // Обновляем продукт в базе данных
                _context.Products.Update(product);
            }

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            _logger.LogInformation("Поиск аналогов завершен.");
        }
    }
}
