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

public class CategoryServiceTests
{
    private AppDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB name for isolation
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
    public async Task GetCategoriesAsync_ShouldReturnAllCategories()
    {
        // Arrange
        using var context = GetDatabaseContext();
        // Clear seeded items if any, though InMemory starts fresh
        context.Categories.RemoveRange(context.Categories);
        context.Categories.Add(new Category { Id = 1, Name = "Cat1", Description = "Desc1" });
        context.Categories.Add(new Category { Id = 2, Name = "Cat2", Description = "Desc2" });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);

        // Act
        var result = await service.GetCategoriesAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, c => c.Name == "Cat1");
        Assert.Contains(result, c => c.Name == "Cat2");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        using var context = GetDatabaseContext();
        context.Categories.Add(new Category { Id = 10, Name = "Electronics" });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);

        // Act
        var result = await service.GetCategoryByIdAsync(10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Electronics", result.Value.Name);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        using var context = GetDatabaseContext();
        var service = new CategoryService(context);

        // Act
        var result = await service.GetCategoryByIdAsync(999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
        Assert.Equal("Category not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldCreateCategory_WhenNameIsUnique()
    {
        // Arrange
        using var context = GetDatabaseContext();
        var service = new CategoryService(context);
        var dto = new CreateCategoryDto { Name = "Fashion", Description = "Clothing items" };

        // Act
        var result = await service.CreateCategoryAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.Id > 0);
        Assert.Equal("Fashion", result.Value.Name);

        // Verify write to DB
        var savedCategory = await context.Categories.FindAsync(result.Value.Id);
        Assert.NotNull(savedCategory);
        Assert.Equal("Fashion", savedCategory.Name);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldFail_WhenNameExists()
    {
        // Arrange
        using var context = GetDatabaseContext();
        context.Categories.Add(new Category { Name = "Fashion" });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var dto = new CreateCategoryDto { Name = "fashion" }; // case-insensitive check

        // Act
        var result = await service.CreateCategoryAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category name already exists.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldUpdate_WhenValid()
    {
        // Arrange
        using var context = GetDatabaseContext();
        var category = new Category { Id = 1, Name = "OldName", Description = "OldDesc" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var dto = new CreateCategoryDto { Name = "NewName", Description = "NewDesc" };

        // Act
        var result = await service.UpdateCategoryAsync(1, dto);

        // Assert
        Assert.True(result.IsSuccess);
        
        var updated = await context.Categories.FindAsync(1);
        Assert.Equal("NewName", updated?.Name);
        Assert.Equal("NewDesc", updated?.Description);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldFail_WhenDuplicateName()
    {
        // Arrange
        using var context = GetDatabaseContext();
        context.Categories.Add(new Category { Id = 1, Name = "Cat1" });
        context.Categories.Add(new Category { Id = 2, Name = "Cat2" });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);
        var dto = new CreateCategoryDto { Name = "cat2" }; // Try to update Cat1's name to Cat2

        // Act
        var result = await service.UpdateCategoryAsync(1, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category name already exists.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldDelete_WhenNoAssociatedProducts()
    {
        // Arrange
        using var context = GetDatabaseContext();
        context.Categories.Add(new Category { Id = 1, Name = "EmptyCategory" });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);

        // Act
        var result = await service.DeleteCategoryAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        var category = await context.Categories.FindAsync(1);
        Assert.Null(category);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldFail_WhenHasAssociatedProducts()
    {
        // Arrange
        using var context = GetDatabaseContext();
        context.Categories.Add(new Category { Id = 5, Name = "HasProducts" });
        context.Products.Add(new Product { Id = 10, Name = "ProductA", CategoryId = 5, Price = 100, Stock = 5 });
        await context.SaveChangesAsync();

        var service = new CategoryService(context);

        // Act
        var result = await service.DeleteCategoryAsync(5);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot delete this category because there are active products linked to it.", result.ErrorMessage);
        
        // Category should still exist in DB
        var category = await context.Categories.FindAsync(5);
        Assert.NotNull(category);
    }
}
