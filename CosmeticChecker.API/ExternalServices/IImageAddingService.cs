namespace CosmeticChecker.API.ExternalServices
{
    public interface IImageAddingService
    {
        Task UpdateProductImageUrlsFromJsonAsync(string jsonFilePath); // Метод для обновления ImageURL
    }
}
