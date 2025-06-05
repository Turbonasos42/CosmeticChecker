namespace CosmeticChecker.API.DTOs
{
    public class FavoriteProductDto
    {
        public int Id { get; set; } // ID продукта
        public string Name { get; set; } // Название продукта
        public string Brand { get; set; } // Бренд продукта
        public decimal Price { get; set; } // Цена продукта
        public string ProductLink { get; set; } // Добавлено свойство ProductLink

    }
}
