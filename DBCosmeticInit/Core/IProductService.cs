using System.Threading.Tasks;

namespace Core
{
    public interface IProductService
    {
        Task LoadProductsFromJsonAsync(string filePath);
    }
}