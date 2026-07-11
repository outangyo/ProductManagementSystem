using System.ComponentModel.DataAnnotations;

namespace ProductManagementSystem.API.DTOs;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "กรุณากรอกชื่อหมวดหมู่")]
    [StringLength(100, ErrorMessage = "ชื่อหมวดหมู่ต้องมีความยาวไม่เกิน 100 ตัวอักษร")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "รายละเอียดต้องมีความยาวไม่เกิน 500 ตัวอักษร")]
    public string? Description { get; set; }
}
