# Product Management System (Mini Project)

ระบบจัดการสินค้าภายในองค์กร พัฒนาด้วย C# .NET 10 Web API และ Angular v22 พร้อมระบบความปลอดภัย JWT Authentication และฐานข้อมูล SQL Server

## โครงสร้างโปรเจค (Project Structure)

โปรเจคนี้จัดระเบียบในรูปแบบ Multi-Project Solution ประกอบไปด้วย:
* **ProductManagementSystem.API:** ASP.NET Core Web API (.NET 10) ทำหน้าที่เป็น Backend Service
* **ProductManagementSystem.Db:** คลาสไลบรารี (.NET 10) เก็บ Entity Framework Core (Code First) และการเชื่อมต่อ SQL Server
* **ProductManagementSystem.Web:** โปรเจค Frontend พัฒนาด้วย Angular v22 (Standalone Components)

---

## ขั้นตอนการรันระบบ (Getting Started)

### 1. ฝั่ง Backend
1. ตั้งค่า Connection String ใน `ProductManagementSystem.API/appsettings.json`
2. รันคำสั่งสร้างฐานข้อมูล:
   ```bash
   dotnet ef database update --project ProductManagementSystem.API
   ```
3. รัน API:
   ```bash
   dotnet run --project ProductManagementSystem.API
   ```

### 2. ฝั่ง Frontend
1. เข้าโฟลเดอร์เว็บ: `cd ProductManagementSystem.Web`
2. ติดตั้ง Library: `npm install`
3. รันเว็บ: `npm start`
