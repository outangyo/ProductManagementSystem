using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.API.DTOs;
using ProductManagementSystem.API.Services;

namespace ProductManagementSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")] // /api/categories (อิงตามชื่อคลาส Categories)
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    // การดึงระบบจัดการบริการ (CategoryService) เข้ามาใช้งานแทนการเข้าหา DbContext ตรงๆ
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: /api/categories (ดึงข้อมูลหมวดหมู่ทั้งหมด)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _categoryService.GetCategoriesAsync();
        return Ok(categories); // ส่ง HTTP Status Code 200 OK กลับไปพร้อมข้อมูล
    }

    // GET: /api/categories/{id} (ดึงข้อมูลหมวดหมู่รายตัวตาม ID)
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        if (result.IsNotFound)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    // POST: /api/categories (เพิ่มหมวดหมู่ใหม่)
    [Authorize(Roles = "Admin")] // ยอมรับเฉพาะ Admin
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateCategoryAsync(dto);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        // ส่งสถานะ 201 Created กลับไป พร้อมระบุ Location ของข้อมูลตัวนี้
        return CreatedAtAction(nameof(GetCategory), new { id = result.Value!.Id }, result.Value);
    }

    // PUT: /api/categories/{id} (แก้ไขหมวดหมู่สินค้า)
    [Authorize(Roles = "Admin")] // ยอมรับเฉพาะ Admin
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, dto);
        if (result.IsNotFound)
        {
            return NotFound(new { message = result.ErrorMessage });
        }
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        // คืนสถานะ 204 No Content ตามมาตรฐานของ PUT เมื่อแก้ไขสำเร็จ
        return NoContent();
    }

    // DELETE: /api/categories/{id} (ลบหมวดหมู่สินค้า)
    [Authorize(Roles = "Admin")] // ยอมรับเฉพาะ Admin
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        if (result.IsNotFound)
        {
            return NotFound(new { message = result.ErrorMessage });
        }
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return NoContent();
    }
}
