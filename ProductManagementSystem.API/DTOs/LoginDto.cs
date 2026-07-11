using System.ComponentModel.DataAnnotations;

namespace ProductManagementSystem.API.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "กรุณากรอก Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรุณากรอก Password")]
    public string Password { get; set; } = string.Empty;
}