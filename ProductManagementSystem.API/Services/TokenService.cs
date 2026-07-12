using Microsoft.IdentityModel.Tokens;
using ProductManagementSystem.Db.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProductManagementSystem.API.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        // 1. ดึงคีย์ลับจาก User Secrets และนำมาแปรรูปเป็นกุญแจเข้ารหัส
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        // 2. กำหนดขั้นตอนการเซ็นลายเซ็นดิจิทัลด้วยอัลกอริทึม SHA256
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        // 3. แนบข้อมูลของผู้ใช้เข้าไปในตั๋ว (เรียกว่า Claims)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("FullName", user.FullName ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role) // แนบสิทธิ์การใช้งาน (Admin/User) ลงใน Claim
        };

        // 4. ประกอบร่างตั๋ว JWT (ตั้งเวลาหมดอายุ, ระบุผู้ออกตั๋ว, ผู้รับตั๋ว, และเซ็นชื่อกำกับ)
        var expiryMinutes = double.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            SigningCredentials = creds,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        // 5. สั่งให้เขียนตั๋วออกมาเป็น String ยาว ๆ ส่งกลับไปใช้งาน
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // สำหรับแกะตั๋วหมดอายุ ด่านพวกนี้ข้ามได้เลย
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = false // สำคัญมาก: ห้ามดักจับหมดอายุเพราะเราจงใจแกะตั๋วที่หมดอายุแล้ว
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid access token signature.");
        }

        return principal;
    }
}
