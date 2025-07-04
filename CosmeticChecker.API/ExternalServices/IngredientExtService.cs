using DatabaseContext;
using DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CosmeticChecker.API.ExternalServices
{
    public class IngredientExtService : IIngredientExtService
    {
        private readonly AppDbContext _context;
        private readonly Random _random = new Random(); // Инициализация Random для генерации случайных чисел
        private readonly ILogger<IngredientExtService> _logger;

        public IngredientExtService(AppDbContext context, ILogger<IngredientExtService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Метод для загрузки ингредиентов из JSON файла и добавления их в БД
        public async Task AddIngredientsFromJsonAsync(string jsonFilePath)
        {
            var jsonData = await File.ReadAllTextAsync(jsonFilePath);  // Чтение JSON файла
            var ingredientsList = JsonConvert.DeserializeObject<List<Ingredient>>(jsonData);  // Десериализация JSON

            foreach (var ingredient in ingredientsList)
            {
                // Генерируем случайное значение для SafetyLevel от 1 до 10
                ingredient.SafetyLevel = _random.Next(1, 11);  // Значение от 1 до 10

                // Проверяем, существует ли уже ингредиент в базе данных
                var existingIngredient = await _context.Ingredients
                    .FirstOrDefaultAsync(i => i.Name == ingredient.Name);

                if (existingIngredient == null)
                {
                    // Если ингредиент не найден, добавляем новый
                    _context.Ingredients.Add(ingredient);
                }
                else
                {
                    // Если ингредиент уже существует, обновляем его описание и SafetyLevel
                    existingIngredient.Description = ingredient.Description;
                    existingIngredient.SafetyLevel = ingredient.SafetyLevel; // Обновляем SafetyLevel
                }
            }

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();
        }


        public async Task LinkAllIngredientsToProductsAsync()
        {
            _logger.LogInformation("Начинаем связывание ингредиентов с продуктами...");

            // Получаем первые 10 ингредиентов
            var ingredients = await _context.Ingredients
                .ToListAsync();

            if (!ingredients.Any())
            {
                _logger.LogWarning("Нет ингредиентов для связывания с продуктами.");
                return;
            }

            // Получаем все продукты
            var products = await _context.Products
                .ToListAsync();

            if (!products.Any())
            {
                _logger.LogWarning("Нет продуктов для связывания с ингредиентами.");
                return;
            }

            foreach (var ingredient in ingredients)
            {
                // Ищем все продукты, которые содержат этот ингредиент в поле Ingredients
                var matchingProducts = products.Where(p => p.Ingredients.Contains(ingredient.Name)).ToList();

                // Если продукты не найдены для текущего ингредиента, пропускаем его
                if (!matchingProducts.Any())
                {
                    _logger.LogWarning($"Продукты с ингредиентом '{ingredient.Name}' не найдены.");
                    continue;
                }

                // Создаем связи между ингредиентами и продуктами
                foreach (var product in matchingProducts)
                {
                    var productIngredient = new ProductIngredient
                    {
                        ProductId = product.Id,
                        IngredientId = ingredient.Id,


                    };

                    _context.ProductIngredients.Add(productIngredient); // Добавляем связь
                }

                _logger.LogInformation($"Связь для ингредиента '{ingredient.Name}' с продуктами добавлена.");
            }

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            _logger.LogInformation("Связывание ингредиентов с продуктами завершено.");
        }

    

    public async Task AddIngredientToProductAsync(int productId, int ingredientId)
        {
            productId = 1;
            ingredientId = 6000;
            var product = await _context.Products.FindAsync(productId);
            var ingredient = await _context.Ingredients.FindAsync(ingredientId);

            if (product == null || ingredient == null)
            {
                _logger.LogWarning("Продукт или ингредиент не найден.");
                return;
            }

            var productIngredient = new ProductIngredient
            {
                ProductId = productId,
                IngredientId = ingredientId
            };

            _context.ProductIngredients.Add(productIngredient);
            await _context.SaveChangesAsync();
        }

    }
}


