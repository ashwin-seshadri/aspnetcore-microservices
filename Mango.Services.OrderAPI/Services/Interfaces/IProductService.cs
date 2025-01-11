using Mango.Services.OrderAPI.Models.Dto;

namespace Mango.Services.OrderAPI.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProductsAsync();
    }
}
