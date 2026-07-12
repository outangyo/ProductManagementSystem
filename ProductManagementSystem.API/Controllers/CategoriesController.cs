using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagementSystem.API.DTOs;
using ProductManagementSystem.Db.Data;
using ProductManagementSystem.Db.Entities;

namespace ProductManagementSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")] // /api/categories (อิงตามชื่อคลาส Categories)
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    // การดึงระบบฐานข้อมูล (DbContext) เข้ามาใช้งานในคลาสผ่าน Constructor
    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /api/categories (ดึงข้อมูลหมวดหมู่ทั้งหมด)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        // Query ข้อมูลจาก DB และแปลงโครงสร้างเป็น DTO ทันที
        var categories = await _context.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync();

        return Ok(categories); // ส่ง HTTP Status Code 200 OK กลับไปพร้อมข้อมูล
    }

    // GET: /api/categories/{id} (ดึงข้อมูลหมวดหมู่รายตัวตาม ID)
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        
        if (category == null)
        {
            return NotFound(new { message = "Category not found." });
        }

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };

        return Ok(categoryDto);
    }

    // POST: /api/categories (เพิ่มหมวดหมู่ใหม่)
    [Authorize(Roles = "Admin")] // ยอมรับเฉพาะ Admin
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        // 1. ตรวจสอบความปลอดภัยระดับข้อมูล: ตรวจชื่อซ้ำในระบบ (แบบ Case-Insensitive)
        if (await _context.Categories.AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower()))
        {
            return BadRequest(new { message = "Category name already exists." });
        }

        // 2. Map ข้อมูลจาก DTO แปลงเป็น Entity เพื่อเซฟลงฐานข้อมูล
        var category = new Category 
        { 
            Name = dto.Name, 
            Description = dto.Description 
        };
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // 3. แปลงผลลัพธ์กลับไปเป็น DTO เพื่อส่งให้กับฝั่ง Angular
        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };

        // 4. ส่งสถานะ 201 Created กลับไป พร้อมระบุ Location ของข้อมูลตัวนี้
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, categoryDto);
    }

    // PUT: /api/categories/{id} (แก้ไขหมวดหมู่สินค้า)
    [Authorize(Roles = "Admin")] // ยอมรับเฉพาะ Admin
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDto dto)
    {
        // 1. ค้นหาหมวดหมู่สินค้าที่ต้องการแก้ไขในฐานข้อมูล
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Category to update not found." });
        }

        // 2. ตรวจสอบชื่อซ้ำ: ชื่อใหม่ต้องไม่ไปซ้ำกับของหมวดหมู่อื่น (แต่ใช้ชื่อเดิมของตัวเองได้)
        if (await _context.Categories.AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Id != id))
        {
            return BadRequest(new { message = "Category name already exists." });
        }

        // 3. ปรับปรุงค่าฟิลด์ต่าง ๆ ของ Entity
        category.Name = dto.Name;
        category.Description = dto.Description;

        // 4. บันทึกผลการเปลี่ยนแปลงลงฐานข้อมูล
        await _context.SaveChangesAsync();

        // 5. คืนสถานะ 204 No Content ตามมาตรฐานของ PUT เมื่อแก้ไขสำเร็จ
        return NoContent();
    }

    // DELETE: /api/categories/{id} (ลบหมวดหมู่สินค้า)
    [Authorize(Roles = "Admin")] // ยอมรับเฉพาะ Admin
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        // 1. ค้นหาหมวดหมู่สินค้าที่ต้องการลบในฐานข้อมูล
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound(new { message = "Category to delete not found." });
        }

        try
        {
            // 2. ดำเนินการลบข้อมูล Entity
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException)
        {
            // 3. ป้องกันบักระดับ DB: กรณีหมวดหมู่นั้นยังมีสินค้าผูกไว้อยู่ (ผลลัพธ์จาก OnDelete Restrict ที่เราตั้งค่าไว้)
            // จะส่งสถานะ 400 Bad Request พร้อมข้อความเตือนเพื่อให้หน้าบ้านแสดง Dialog แจ้งเตือนผู้ใช้ได้อย่างถูกต้อง
            return BadRequest(new { message = "Cannot delete this category because there are active products linked to it." });
        }
    }
}
