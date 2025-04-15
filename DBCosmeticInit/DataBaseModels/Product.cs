using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatabaseModels
{
    public class Product
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public string ProductType { get; set; }
        public string Ingredients { get; set; }
        public string SkinType { get; set; }
        public decimal Price { get; set; } 
    }
    
}