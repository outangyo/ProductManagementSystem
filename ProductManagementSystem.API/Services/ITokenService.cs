using ProductManagementSystem.Db.Entities;
using System.Security.Claims;

namespace ProductManagementSystem.API.Services;

public interface ITokenService
{
    /// ฟังก์ชันสำหรับสร้าง JWT Token โดยรับข้อมูล User เข้ามาประมวลผล
    string GenerateToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
