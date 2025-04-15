using DatabaseContext;
using DatabaseModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        // Загружаем продукты из JSON файла
        public async Task LoadProductsFromJsonAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Файл JSON не найден.");
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(filePath);

                var products = JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (products == null || products.Count == 0)
                {
                    Console.WriteLine("Файл JSON пуст или имеет неверный формат.");
                    return;
                }

                // Проверка и замена NULL значений на пробел
                foreach (var product in products)
                {
                    
                    if (string.IsNullOrEmpty(product.ProductType))
                    {
                        product.ProductType = " "; 
                    }
                    if (string.IsNullOrEmpty(product.Name))
                    {
                        product.Name = " "; 
                    }
                    if (string.IsNullOrEmpty(product.Brand))
                    {
                        product.Brand = " ";  
                    }
                    if (string.IsNullOrEmpty(product.Category))
                    {
                        product.Category = " "; 
                    }
                    if (string.IsNullOrEmpty(product.Ingredients))
                    {
                        product.Ingredients = " ";  
                    }
                    if (string.IsNullOrEmpty(product.SkinType))
                    {
                        product.SkinType = " ";  
                    }
                    
                    if (product.Price == 0)
                    {
                        product.Price = 0.01m;  // замена на минимальное значение
                    }
                }

                // Очистим таблицу перед загрузкой новых данных
                await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Products\" RESTART IDENTITY");

                // Добавим новые данные
                _context.Products.AddRange(products);
                await _context.SaveChangesAsync();

                Console.WriteLine("Данные успешно загружены из JSON в базу.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке JSON: {ex.Message}");

                // Показать подробности ошибки
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
                }
            }
        }
    }
}
