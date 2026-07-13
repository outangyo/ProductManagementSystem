using ProductManagementSystem.API.DTOs;

namespace ProductManagementSystem.API.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    Task<ServiceResult<CategoryDto>> GetCategoryByIdAsync(int id);
    Task<ServiceResult<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto);
    Task<ServiceResult<bool>> UpdateCategoryAsync(int id, CreateCategoryDto dto);
    Task<ServiceResult<bool>> DeleteCategoryAsync(int id);
}
