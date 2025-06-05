namespace CosmeticChecker.API.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }  // ID продукта
        public string Url { get; set; }  // URL продукта
        public string ImageUrl { get; set; }  // URL изображения продукта
        public string Name { get; set; }  // Название продукта
        public string Brand { get; set; }  // Бренд продукта
        public string Category { get; set; }  // Категория продукта
        public string ProductType { get; set; }  // Тип продукта
        public string Ingredients { get; set; }  // Ингредиенты продукта
        public string SkinType { get; set; }  // Тип кожи
        public decimal Price { get; set; }  // Цена продукта
        public string Description { get; set; }  // Описание продукта
        public double SafetyRating { get; set; }  // Уровень безопасности
        public double UserRatings { get; set; }
        public string Analogues { get; set; }  // Аналоги продукта
    }
    public class ProductListDto
    {
        public List<ProductDto> Products { get; set; }
        public int TotalCount { get; set; }
    }


}
