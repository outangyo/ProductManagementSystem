using System.ComponentModel.DataAnnotations;

namespace ProductManagementSystem.API.DTOs;

public class CreateProductDto
{
    [Required(ErrorMessage = "กรุณากรอกชื่อสินค้า")]
    [StringLength(150, ErrorMessage = "ชื่อสินค้าต้องมีความยาวไม่เกิน 150 ตัวอักษร")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "รายละเอียดต้องมีความยาวไม่เกิน 1000 ตัวอักษร")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "กรุณากรอกราคาสินค้า")]
    [Range(0.01, double.MaxValue, ErrorMessage = "ราคาสินค้าต้องมากกว่า 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "กรุณากรอกจำนวนสินค้าในสต็อก")]
    [Range(0, int.MaxValue, ErrorMessage = "จำนวนสินค้าในสต็อกต้องไม่ต่ำกว่า 0")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "กรุณาระบุหมวดหมู่สินค้า")]
    public int CategoryId { get; set; }
}
