using ProductManagementSystem.Db.Entities;

namespace ProductManagementSystem.API.Services;

public interface ITokenService
{
    /// ฟังก์ชันสำหรับสร้าง JWT Token โดยรับข้อมูล User เข้ามาประมวลผล
    string GenerateToken(User user);
}
