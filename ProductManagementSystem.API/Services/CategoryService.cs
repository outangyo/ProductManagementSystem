using Microsoft.EntityFrameworkCore;
using ProductManagementSystem.API.DTOs;
using ProductManagementSystem.Db.Data;
using ProductManagementSystem.Db.Entities;

namespace ProductManagementSystem.API.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        return await _context.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync();
    }

    public async Task<ServiceResult<CategoryDto>> GetCategoryByIdAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return ServiceResult<CategoryDto>.NotFound("Category not found.");
        }

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };

        return ServiceResult<CategoryDto>.Success(dto);
    }

    public async Task<ServiceResult<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto)
    {
        if (await _context.Categories.AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower()))
        {
            return ServiceResult<CategoryDto>.Failure("Category name already exists.");
        }

        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };

        return ServiceResult<CategoryDto>.Success(categoryDto);
    }

    public async Task<ServiceResult<bool>> UpdateCategoryAsync(int id, CreateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return ServiceResult<bool>.NotFound("Category to update not found.");
        }

        if (await _context.Categories.AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Id != id))
        {
            return ServiceResult<bool>.Failure("Category name already exists.");
        }

        category.Name = dto.Name;
        category.Description = dto.Description;

        await _context.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return ServiceResult<bool>.NotFound("Category to delete not found.");
        }

        // ตรวจสอบความสัมพันธ์: ห้ามลบหากยังมีสินค้าผูกอยู่
        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            return ServiceResult<bool>.Failure("Cannot delete this category because there are active products linked to it.");
        }

        try
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Success(true);
        }
        catch (DbUpdateException)
        {
            return ServiceResult<bool>.Failure("Cannot delete this category because there are active products linked to it.");
        }
    }
}
