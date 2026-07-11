using System.Security.Cryptography;

namespace ProductManagementSystem.API.Helpers;

public static class PasswordHelper
{
    private const int SaltSize = 16; // 128-bit salt
    private const int KeySize = 32;  // 256-bit key
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA512;

    /// <summary>
    /// เข้ารหัสผ่านด้วยวิธี PBKDF2 (สุ่มเกลือใหม่ทุกครั้ง)
    /// </summary>
    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);
        return $"{Convert.ToHexString(salt)}:{Convert.ToHexString(hash)}";
    }

    /// <summary>
    /// ตรวจสอบรหัสผ่านที่ป้อนเข้ามาเทียบกับค่าที่เข้ารหัสไว้ใน DB
    /// </summary>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        string[] parts = hashedPassword.Split(':');
        if (parts.Length != 2) return false;

        byte[] salt = Convert.FromHexString(parts[0]);
        byte[] hash = Convert.FromHexString(parts[1]);

        byte[] testHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);
        return CryptographicOperations.FixedTimeEquals(hash, testHash);
    }
}
