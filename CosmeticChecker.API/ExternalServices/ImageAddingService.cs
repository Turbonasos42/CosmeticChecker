using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using DatabaseContext;
using DatabaseModels;
namespace CosmeticChecker.API.ExternalServices
{
    public class ImageAddingService : IImageAddingService
    {
        private readonly AppDbContext _context;

        public ImageAddingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task UpdateProductImageUrlsFromJsonAsync(string jsonFilePath)
        {
            // Чтение данных из JSON файла
            var jsonData = await File.ReadAllTextAsync(jsonFilePath);
            var productsFromJson = JsonConvert.DeserializeObject<List<ProductFromJson>>(jsonData);

            // Поиск и обновление ссылок на изображения в базе данных
            foreach (var jsonProduct in productsFromJson)
            {
                var productInDb = await _context.Products
                    .Where(p => p.Url == jsonProduct.Url)
                    .FirstOrDefaultAsync();

                if (productInDb != null)
                {
                    productInDb.ImageUrl = jsonProduct.ImageURL;
                    _context.Products.Update(productInDb);
                }
            }

            // Сохранение изменений в базе данных
            await _context.SaveChangesAsync();
        }
    }

    // Модель для десериализации JSON данных
    public class ProductFromJson
    {
        public string Url { get; set; }
        public string ImageURL { get; set; }
    }
}
