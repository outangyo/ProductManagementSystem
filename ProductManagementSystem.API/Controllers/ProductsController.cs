using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagementSystem.API.DTOs;
using ProductManagementSystem.Db.Data;
using ProductManagementSystem.Db.Entities;

namespace ProductManagementSystem.API.Controllers;

[Authorize] // บังคับสิทธิ์ JWT เสมอ
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /api/products (ดึงข้อมูลสินค้าทั้งหมด พร้อมระบบค้นหาและแบ่งหน้า)
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? search = null)
    {
        // 1. ดึงข้อมูลสินค้าพร้อม Join ตารางหมวดหมู่ด้วย Include
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        // 2. กรองข้อมูลเฉพาะชื่อสินค้าที่มีข้อความสะกดตรงกับคำค้นหา
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
        }

        // 3. นับรายการรวมเพื่อใช้คำนวณแบ่งหน้า
        var totalItems = await query.CountAsync();

        // 4. ใช้ Skip และ Take ในการแบ่งช่วงข้อมูลที่ดึง และจัดโครงสร้างใส่ DTO
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

        // 5. คำนวณจำนวนหน้าทั้งหมด
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        // 6. ตอบกลับข้อมูลพร้อมโครงสร้าง Pagination
        return Ok(new
        {
            items = products,
            totalItems = totalItems,
            currentPage = page,
            pageSize = pageSize,
            totalPages = totalPages
        });
    }

    // GET: /api/products/{id} (ดึงข้อมูลสินค้ารายตัวตาม ID)
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        // 1. ค้นหาสินค้าตาม ID พร้อมดึงตาราง Category (Include) เพื่อดึงชื่อหมวดหมู่มาร่วมด้วย
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        // 2. หากไม่พบสินค้า ให้ส่ง HTTP 404 กลับพร้อมข้อความเตือน
        if (product == null)
        {
            // ส่ง HTTP 404 NotFound
            return NotFound(new { message = "Product not found." });
        }

        // 3. แปลงข้อมูลดิบ Entity เป็น DTO เพื่อส่งให้กับ Angular
        var productDto = new ProductDto
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

        return Ok(productDto);
    }

    // POST: /api/products (เพิ่มสินค้าใหม่)
    [Authorize(Roles = "Admin")] // ยอมรับเฉพาะ Admin
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
    {
        // 1. ตรวจสอบความถูกต้องระดับความสัมพันธ์ (Foreign Key): ตรวจว่ามีหมวดหมู่ ID นี้ในระบบจริงไหม
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            return BadRequest(new { message = "The specified category is invalid or does not exist." });
        }

        // 2. แปลง DTO เป็น Entity และสั่งบันทึกเข้าฐานข้อมูล
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

        // 3. โหลดข้อมูลหมวดหมู่เพื่อนำชื่อหมวดหมู่ (CategoryName) ไปป้อนให้กับ DTO ส่งกลับไปให้ Angular
        var category = await _context.Categories.FindAsync(dto.CategoryId);
        var categoryName = category?.Name ?? string.Empty;

        // 4. จัดรูปแบบข้อมูลออกในฐานะ DTO
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

        // 5. ส่งสถานะ 201 Created กลับไป พร้อมชี้ปลายทาง Location ไปที่ปุ่มดูรายละเอียด (GetProduct)
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
    }

    // PUT: /api/products/{id} (แก้ไขข้อมูลสินค้า)
    [Authorize(Roles = "Admin")] // ยอมรับเฉพาะ Admin
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] CreateProductDto dto)
    {
        // 1. ค้นหาสินค้าที่ต้องการแก้ไขในฐานข้อมูล
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = "Product to update not found." });
        }

        // 2. ตรวจสอบความถูกต้องระดับความสัมพันธ์ (Foreign Key): ตรวจว่ามีหมวดหมู่ ID ใหม่นี้ในระบบจริงไหม
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists)
        {
            return BadRequest(new { message = "The specified category is invalid or does not exist." });
        }

        // 3. ปรับปรุงค่าฟิลด์ต่าง ๆ ของ Entity
        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;
        product.ImageUrl = dto.ImageUrl;

        // 4. บันทึกผลการเปลี่ยนแปลงลงฐานข้อมูล
        await _context.SaveChangesAsync();

        // 5. คืนสถานะ 204 No Content ตามมาตรฐานของ PUT เมื่อแก้ไขข้อมูลสำเร็จ
        return NoContent();
    }

    // DELETE: /api/products/{id} (ลบสินค้า)
    [Authorize(Roles = "Admin")] // ยอมรับเฉพาะ Admin
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        // 1. ค้นหาสินค้าที่ต้องการลบในฐานข้อมูล
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = "Product to delete not found." });
        }

        // 2. ลบข้อมูลสินค้าออกจากฐานข้อมูล
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        // 3. คืนสถานะ 204 No Content เพื่อชี้ว่าลบสำเร็จแล้ว
        return NoContent();
    }

    // POST: /api/products/upload (อัปโหลดรูปภาพสินค้า)
    [Authorize(Roles = "Admin")] // อนุญาตเฉพาะ Admin
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        // 1. ตรวจสอบว่ามีไฟล์ส่งมาจริงไหม
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded." });
        }

        // 2. จำกัดขนาดไฟล์ห้ามเกิน 2MB
        if (file.Length > 2 * 1024 * 1024)
        {
            return BadRequest(new { message = "File size cannot exceed 2MB." });
        }

        // 3. กรองสกุลไฟล์ บังคับเฉพาะรูปภาพ
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Only JPG, JPEG, PNG, and WEBP images are allowed." });
        }

        // 4. เตรียมที่จัดเก็บโฟลเดอร์ wwwroot/uploads
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // 5. สุ่มตั้งชื่อไฟล์ใหม่ด้วย GUID ป้องกันชื่อซ้ำ
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // 6. เขียนไฟล์รูปภาพลงดิสก์ของเซิร์ฟเวอร์
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 7. คืนค่า URL สมบูรณ์กลับไปให้หน้าบ้าน (เช่น https://localhost:7133/uploads/xxx.png)
        var request = HttpContext.Request;
        var imageUrl = $"{request.Scheme}://{request.Host}/uploads/{uniqueFileName}";

        return Ok(new { imageUrl });
    }
}
