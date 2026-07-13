using ProductManagementSystem.API.DTOs;

namespace ProductManagementSystem.API.Services;

public interface IProductService
{
    Task<PaginatedProductsDto> GetProductsAsync(int page, int pageSize, string? search);
    Task<ServiceResult<ProductDto>> GetProductByIdAsync(int id);
    Task<ServiceResult<ProductDto>> CreateProductAsync(CreateProductDto dto);
    Task<ServiceResult<bool>> UpdateProductAsync(int id, CreateProductDto dto);
    Task<ServiceResult<bool>> DeleteProductAsync(int id);
}
