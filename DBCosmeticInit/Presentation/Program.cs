using DatabaseContext;
using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace Presentation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=tazz6nce;Port=5432"))
                .AddScoped<IProductService, ProductService>()
                .BuildServiceProvider();

            var productService = serviceProvider.GetService<IProductService>();

            if (productService == null)
            {
                Console.WriteLine("Ошибка в получении сервиса ProductService.");
                return;
            }

            bool continueRunning = true;
            while (continueRunning)
            {
                // Главное меню с инструкциями для пользователя
                Console.WriteLine("\n--- Главное меню ---");
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1. Загрузить продукты из JSON");
                Console.WriteLine("0. Выход");

                // Чтение выбора пользователя
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("Введите путь к JSON-файлу:");
                        var filePath = Console.ReadLine();
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            await productService.LoadProductsFromJsonAsync(filePath);
                        }
                        else
                        {
                            Console.WriteLine("Путь к файлу не может быть пустым.");
                        }
                        break;

                    case "0":
                        Console.WriteLine("Выход из программы...");
                        continueRunning = false;
                        break;

                    default:
                        Console.WriteLine("Некорректный выбор. Пожалуйста, выберите действие из списка.");
                        break;
                }
            }
        }
    }
}
