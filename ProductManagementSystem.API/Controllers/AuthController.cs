using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagementSystem.API.DTOs;
using ProductManagementSystem.API.Helpers;
using ProductManagementSystem.API.Services;
using ProductManagementSystem.Db.Data;

namespace ProductManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")] // เส้นทาง: /api/auth
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(AppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("login")] // POST: /api/auth/login
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        // 1. ค้นหาผู้ใช้งานจาก Username ในฐานข้อมูล (แบบไม่สนใจพิมพ์เล็ก-พิมพ์ใหญ่)
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == loginDto.Username.ToLower());
        
        // 2. ตรวจสอบผู้ใช้งานและเปรียบเทียบรหัสผ่านด้วย PasswordHelper
        if (user == null || !PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            // ส่งข้อความปฏิเสธการเข้าถึงกลับไปแบบทั่วไปเพื่อความปลอดภัย (Generic Error Message)
            return Unauthorized(new { message = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง" });
        }

        // 3. เมื่อรหัสผ่านถูกต้อง ให้สั่งปั๊มตั๋ว Token ออกมา
        var token = _tokenService.GenerateToken(user);

        // 4. แพ็คข้อมูลใส่ DTO ส่งกลับไปให้ Angular
        var response = new AuthResponseDto
        {
            Username = user.Username,
            FullName = user.FullName ?? string.Empty,
            Token = token
        };

        return Ok(response);
    }
}
