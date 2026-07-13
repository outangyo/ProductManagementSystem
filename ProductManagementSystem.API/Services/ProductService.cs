using Microsoft.EntityFrameworkCore;
using ProductManagementSystem.API.DTOs;
using ProductManagementSystem.Db.Data;
using ProductManagementSystem.Db.Entities;

namespace ProductManagementSystem.API.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedProductsDto> GetProductsAsync(int page, int pageSize, string? search)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
        }

        var totalItems = await query.CountAsync();

        var products = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync();

        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        return new PaginatedProductsDto
        {
            Items = products,
            TotalItems = totalItems,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<ServiceResult<ProductDto>> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return ServiceResult<ProductDto>.NotFound("Product not found.");
        }

        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            ImageUrl = product.ImageUrl
        };

        return ServiceResult<ProductDto>.Success(dto);
    }

    public async Task<ServiceResult<ProductDto>> CreateProductAsync(CreateProductDto dto)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            return ServiceResult<ProductDto>.Failure("The specified category is invalid or does not exist.");
        }

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId,
            ImageUrl = dto.ImageUrl
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var category = await _context.Categories.FindAsync(dto.CategoryId);
        var categoryName = category?.Name ?? string.Empty;

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CategoryName = categoryName,
            ImageUrl = product.ImageUrl
        };

        return ServiceResult<ProductDto>.Success(productDto);
    }

    public async Task<ServiceResult<bool>> UpdateProductAsync(int id, CreateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return ServiceResult<bool>.NotFound("Product to update not found.");
        }

        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            return ServiceResult<bool>.Failure("The specified category is invalid or does not exist.");
        }

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;
        product.ImageUrl = dto.ImageUrl;

        await _context.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return ServiceResult<bool>.NotFound("Product to delete not found.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }
}
