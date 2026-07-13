using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using ProductManagementSystem.API.DTOs;
using ProductManagementSystem.API.Services;
using System.IO;
using System;
using System.Threading.Tasks;

namespace ProductManagementSystem.API.Controllers;

[Authorize] // บังคับสิทธิ์ JWT เสมอ
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IWebHostEnvironment _env;

    // การดึงระบบจัดการบริการ (ProductService) และ IWebHostEnvironment เข้ามาใช้งาน
    public ProductsController(IProductService productService, IWebHostEnvironment env)
    {
        _productService = productService;
        _env = env;
    }

    // GET: /api/products (ดึงข้อมูลสินค้าทั้งหมด พร้อมระบบค้นหาและแบ่งหน้า)
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? search = null)
    {
        var result = await _productService.GetProductsAsync(page, pageSize, search);
        
        // แปลง ImageUrl ให้เป็น URL แบบ Absolute
        foreach (var item in result.Items)
        {
            item.ImageUrl = GetAbsoluteImageUrl(item.ImageUrl);
        }

        return Ok(new
        {
            items = result.Items,
            totalItems = result.TotalItems,
            currentPage = result.CurrentPage,
            pageSize = result.PageSize,
            totalPages = result.TotalPages
        });
    }

    // GET: /api/products/{id} (ดึงข้อมูลสินค้ารายตัวตาม ID)
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        if (result.IsNotFound)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        // แปลง ImageUrl ให้เป็น URL แบบ Absolute
        result.Value!.ImageUrl = GetAbsoluteImageUrl(result.Value.ImageUrl);

        return Ok(result.Value);
    }

    // POST: /api/products (เพิ่มสินค้าใหม่)
    [Authorize(Roles = "Admin")] // ยอมรับเฉพาะ Admin
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
    {
        var result = await _productService.CreateProductAsync(dto);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        // แปลง ImageUrl ให้เป็น URL แบบ Absolute
        result.Value!.ImageUrl = GetAbsoluteImageUrl(result.Value.ImageUrl);

        // ส่งสถานะ 201 Created กลับไป พร้อมชี้ปลายทาง Location ไปที่ปุ่มดูรายละเอียด (GetProduct)
        return CreatedAtAction(nameof(GetProduct), new { id = result.Value!.Id }, result.Value);
    }

    // PUT: /api/products/{id} (แก้ไขข้อมูลสินค้า)
    [Authorize(Roles = "Admin")] // ยอมรับเฉพาะ Admin
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] CreateProductDto dto)
    {
        var result = await _productService.UpdateProductAsync(id, dto);
        if (result.IsNotFound)
        {
            return NotFound(new { message = result.ErrorMessage });
        }
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        // คืนสถานะ 204 No Content ตามมาตรฐานของ PUT เมื่อแก้ไขข้อมูลสำเร็จ
        return NoContent();
    }

    // DELETE: /api/products/{id} (ลบสินค้า)
    [Authorize(Roles = "Admin")] // ยอมรับเฉพาะ Admin
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        if (result.IsNotFound)
        {
            return NotFound(new { message = result.ErrorMessage });
        }

        // คืนสถานะ 204 No Content เพื่อชี้ว่าลบสำเร็จแล้ว
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

        // 4. เตรียมที่จัดเก็บโฟลเดอร์ WebRoot/uploads
        var webRoot = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }
        var uploadsFolder = Path.Combine(webRoot, "uploads");
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

    private string? GetAbsoluteImageUrl(string? relativeUrl)
    {
        if (string.IsNullOrEmpty(relativeUrl)) return null;
        if (relativeUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
            relativeUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return relativeUrl;
        }
        var request = HttpContext.Request;
        return $"{request.Scheme}://{request.Host}{relativeUrl}";
    }
}
