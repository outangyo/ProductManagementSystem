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

        // 3. เมื่อรหัสผ่านถูกต้อง ให้สั่งปั๊มตั๋ว Token และ Refresh Token ออกมา
        var token = _tokenService.GenerateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // 4. บันทึก Refresh Token และวันหมดอายุลงฐานข้อมูล
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Refresh token มีอายุ 7 วัน
        await _context.SaveChangesAsync();

        // 5. แพ็คข้อมูลใส่ DTO ส่งกลับไปให้ Angular
        var response = new AuthResponseDto
        {
            Username = user.Username,
            FullName = user.FullName ?? string.Empty,
            Token = token,
            RefreshToken = refreshToken
        };

        return Ok(response);
    }

    [HttpPost("refresh")] // POST: /api/auth/refresh
    public async Task<IActionResult> Refresh([FromBody] TokenRequestDto tokenRequestDto)
    {
        if (tokenRequestDto == null)
        {
            return BadRequest(new { message = "ข้อมูลคำขอไม่ถูกต้อง" });
        }

        string accessToken = tokenRequestDto.AccessToken;
        string refreshToken = tokenRequestDto.RefreshToken;

        try
        {
            // 1. แกะค่า Claims (ดึงข้อมูลผู้ใช้) จากตั๋วหลักที่ตายแล้ว
            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var username = principal.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { message = "ตั๋วหลักไม่มีข้อมูลผู้ใช้" });
            }

            // 2. ค้นหาผู้ใช้งานจากฐานข้อมูล
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            // 3. ตรวจสอบเงื่อนไขความปลอดภัย:
            // - มีผู้ใช้จริงไหม
            // - Refresh Token ใน DB ตรงกับที่ส่งมาไหม
            // - Refresh Token ใน DB หมดอายุหรือยัง
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest(new { message = "การเข้าสู่ระบบหมดอายุ หรือ Token ไม่ถูกต้อง กรุณาล็อกอินใหม่" });
            }

            // 4. สั่งปั๊ม Access Token ใบใหม่
            var newAccessToken = _tokenService.GenerateToken(user);
            
            // 5. สั่งสับเปลี่ยน Refresh Token ใบใหม่ (Rotate) เพื่อความปลอดภัยป้องกันการโจรกรรมตั๋วเก่ามาใช้ซ้ำ
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // 6. อัปเดตข้อมูลตัวต่ออายุตัวใหม่ลงใน Database
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            // 7. ตอบกลับสิทธิ์การใช้งานรอบใหม่ให้หน้าบ้าน
            return Ok(new AuthResponseDto
            {
                Username = user.Username,
                FullName = user.FullName ?? string.Empty,
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        catch (Exception)
        {
            return BadRequest(new { message = "เกิดข้อผิดพลาดในการตรวจสอบสิทธิ์ กรุณาล็อกอินใหม่" });
        }
    }
}
