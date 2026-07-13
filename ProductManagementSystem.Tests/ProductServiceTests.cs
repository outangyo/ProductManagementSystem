using Microsoft.EntityFrameworkCore;
using ProductManagementSystem.API.DTOs;
using ProductManagementSystem.API.Services;
using ProductManagementSystem.Db.Data;
using ProductManagementSystem.Db.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProductManagementSystem.Tests;

public class ProductServiceTests
{
    private AppDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        // เคลียร์ข้อมูล Seed เพื่อป้องกันการชนกันของ Key (เช่น ID = 1) ในกรณีจำลองข้อมูลของ Test Cases
        context.Products.RemoveRange(context.Products);
        context.Categories.RemoveRange(context.Categories);
        context.Users.RemoveRange(context.Users);
        context.SaveChanges();
        
        return context;
    }

    [Fact]
    public async Task GetProductsAsync_ShouldReturnPaginatedAndSearchFilteredProducts()
    {
        // Arrange
        using var context = GetDatabaseContext();
        // Clear default seeded items for precise assertions
        context.Products.RemoveRange(context.Products);
        context.Categories.RemoveRange(context.Categories);

        context.Categories.Add(new Category { Id = 1, Name = "Electronics" });
        context.Products.Add(new Product { Id = 1, Name = "iPhone 15", CategoryId = 1, Price = 30000, Stock = 10 });
        context.Products.Add(new Product { Id = 2, Name = "MacBook Pro", CategoryId = 1, Price = 60000, Stock = 5 });
        context.Products.Add(new Product { Id = 3, Name = "Samsung TV", CategoryId = 1, Price = 15000, Stock = 12 });
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        // Act - Test Search filter
        var searchResult = await service.GetProductsAsync(page: 1, pageSize: 10, search: "mac");
        
        // Assert - Search
        Assert.Equal(1, searchResult.TotalItems);
        Assert.Single(searchResult.Items);
        Assert.Equal("MacBook Pro", searchResult.Items.First().Name);

        // Act - Test Pagination
        var paginatedResult = await service.GetProductsAsync(page: 2, pageSize: 2, search: "");

        // Assert - Pagination
        Assert.Equal(3, paginatedResult.TotalItems);
        Assert.Equal(2, paginatedResult.TotalPages);
        Assert.Single(paginatedResult.Items); // Page 2 with size 2 should have 1 item remaining
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnProduct_WhenExists()
    {
        // Arrange
        using var context = GetDatabaseContext();
        context.Categories.Add(new Category { Id = 1, Name = "Electronics" });
        context.Products.Add(new Product { Id = 100, Name = "Headphones", CategoryId = 1, Price = 1500, Stock = 20 });
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        // Act
        var result = await service.GetProductByIdAsync(100);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Headphones", result.Value.Name);
        Assert.Equal("Electronics", result.Value.CategoryName);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        using var context = GetDatabaseContext();
        var service = new ProductService(context);

        // Act
        var result = await service.GetProductByIdAsync(999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
        Assert.Equal("Product not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldSaveProduct_WhenCategoryExists()
    {
        // Arrange
        using var context = GetDatabaseContext();
        context.Categories.Add(new Category { Id = 3, Name = "Fashion" });
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var dto = new CreateProductDto
        {
            Name = "Running Shoes",
            Price = 2500.50m,
            Stock = 50,
            CategoryId = 3,
            Description = "Sports shoes"
        };

        // Act
        var result = await service.CreateProductAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.Id > 0);
        Assert.Equal("Running Shoes", result.Value.Name);
        Assert.Equal("Fashion", result.Value.CategoryName);

        // Verify database write
        var saved = await context.Products.FindAsync(result.Value.Id);
        Assert.NotNull(saved);
        Assert.Equal(2500.50m, saved.Price);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldFail_WhenCategoryDoesNotExist()
    {
        // Arrange
        using var context = GetDatabaseContext();
        var service = new ProductService(context);
        var dto = new CreateProductDto
        {
            Name = "Ghost Product",
            Price = 100,
            Stock = 1,
            CategoryId = 999 // Invalid Category ID
        };

        // Act
        var result = await service.CreateProductAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("The specified category is invalid or does not exist.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateProduct_WhenValid()
    {
        // Arrange
        using var context = GetDatabaseContext();
        context.Categories.Add(new Category { Id = 1, Name = "OldCat" });
        context.Categories.Add(new Category { Id = 2, Name = "NewCat" });
        context.Products.Add(new Product { Id = 1, Name = "OldName", CategoryId = 1, Price = 100, Stock = 5 });
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var dto = new CreateProductDto
        {
            Name = "NewName",
            Price = 200,
            Stock = 10,
            CategoryId = 2,
            ImageUrl = "http://example.com/image.jpg"
        };

        // Act
        var result = await service.UpdateProductAsync(1, dto);

        // Assert
        Assert.True(result.IsSuccess);

        var updated = await context.Products.FindAsync(1);
        Assert.NotNull(updated);
        Assert.Equal("NewName", updated.Name);
        Assert.Equal(2, updated.CategoryId);
        Assert.Equal(200, updated.Price);
        Assert.Equal("http://example.com/image.jpg", updated.ImageUrl);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldFail_WhenCategoryDoesNotExist()
    {
        // Arrange
        using var context = GetDatabaseContext();
        context.Categories.Add(new Category { Id = 1, Name = "Cat1" });
        context.Products.Add(new Product { Id = 1, Name = "Product", CategoryId = 1, Price = 10, Stock = 2 });
        await context.SaveChangesAsync();

        var service = new ProductService(context);
        var dto = new CreateProductDto
        {
            Name = "Product",
            CategoryId = 999 // Invalid Category ID
        };

        // Act
        var result = await service.UpdateProductAsync(1, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("The specified category is invalid or does not exist.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldRemoveProduct_WhenExists()
    {
        // Arrange
        using var context = GetDatabaseContext();
        context.Categories.Add(new Category { Id = 1, Name = "Cat1" });
        context.Products.Add(new Product { Id = 50, Name = "TrashItem", CategoryId = 1, Price = 1, Stock = 1 });
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        // Act
        var result = await service.DeleteProductAsync(50);

        // Assert
        Assert.True(result.IsSuccess);
        
        var deleted = await context.Products.FindAsync(50);
        Assert.Null(deleted);
    }
}
