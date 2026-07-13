# Progress Track

บันทึกสถานะการพัฒนาปัจจุบันและงานที่ต้องทำถัดไป

## เป้าหมายหลัก ณ ตอนนี้ (Active Target)
* ไม่มีเป้าหมายคงค้าง (ทุกสเต็ปพัฒนาและส่งมอบงานเสร็จสมบูรณ์เรียบร้อยแล้ว 100%)

## ประวัติรายการที่ทำเสร็จแล้ว (Completed Tasks)
* **การจัดทำเอกสารคู่มือ (Documentation):**
  * [x] เขียนเอกสารคู่มือการติดตั้งและการใช้งานโครงการใน [README.md](file:///C:/Project/ProductManagementSystem/README.md) สำหรับคนตรวจงานเสร็จเรียบร้อย
* **การทำ Dockerize:**
  * [x] บรรจุตู้คอนเทนเนอร์ (Dockerize) ทั้งฝั่ง Backend, Frontend (Nginx) และ Database ด้วย docker-compose.yml พร้อมแก้ไขปัญหาเครือข่าย IPv6 และการตั้งค่ารุ่น Node.js v22 เสร็จสิ้นสมบูรณ์แล้ว

* **การออกแบบฐานข้อมูลและโครงสร้างข้อมูล (Database & Entities):**
  * [x] ออกแบบและสร้าง Entities: User, Category, Product ในโปรเจค `.Db`
  * [x] สร้าง `AppDbContext.cs` พร้อมกำหนดกติกาการเชื่อมโยงข้อมูล (Fluent API) และ Seed Data เริ่มต้น
  * [x] รัน Migration (`InitialCreate`) และอัปเดตตารางเข้าสู่ฐานข้อมูลจริงบน SQL Server (localhost) เรียบร้อย

* **ระบบความปลอดภัยและการยืนยันตัวตน (Authentication & Authorization):**
  * [x] ดึงข้อมูลความลับ JWT Key ออกไปเก็บใน User Secrets เพื่อความปลอดภัย และล้างคีย์ใน `appsettings.json` เป็นค่าว่าง
  * [x] ลงทะเบียนบริการ JWT Bearer Authentication และติดตั้ง Middleware ใน `Program.cs` ฝั่ง API
  * [x] สร้างระบบ TokenService (ITokenService, TokenService) เพื่อออกตั๋ว JWT
  * [x] สร้าง `AuthController.cs` สำหรับ API `/api/auth/login`
  * [x] พัฒนาระบบต่ออายุสิทธิ์หลักด้วยตั๋วสำรอง (Refresh Token) พร้อมกระบวนการหมุนเวียนคีย์ (Token Rotation) ในฝั่งหลังบ้าน
  * [x] พัฒนาระบบแบ่งบทบาทความปลอดภัยหลังบ้าน (Role-based Authorization) โดยล็อกสิทธิ์การเขียน/ลบ (POST, PUT, DELETE) ให้เฉพาะ Admin และเปิดให้อ่านได้อย่างเดียว (GET) สำหรับ User

* **ระบบการจัดการสินค้าและหมวดหมู่ (Product & Category CRUD APIs):**
  * [x] ลบโค้ดตัวอย่าง WeatherForecast (Clean up boilerplate) ออกจากโปรเจค API
  * [x] สร้าง DTOs สำหรับ Category และ Product พร้อมระบบ Validation (ราคา > 0, สต็อก >= 0)
  * [x] สร้าง `CategoriesController.cs` สำหรับจัดการหมวดหมู่สินค้า (CRUD 5 endpoints) พร้อมความปลอดภัย JWT และระบบดักตรวจข้อผิดพลาดเมื่อลบหมวดหมู่ที่มีสินค้าผูกอยู่
  * [x] พัฒนาระบบ Product CRUD API (สร้าง `ProductsController.cs` พร้อมฟังก์ชันค้นหา, คัดกรอง และแบ่งหน้า Pagination)
  * [x] พัฒนาระบบอัปโหลดรูปภาพสินค้าจริง (File Upload) ทั้งหน้าบ้านและหลังบ้านเรียบร้อย

* **ระบบฝั่งหน้าบ้าน (Frontend - Angular v22):**
  * [x] พัฒนาหน้าบ้านฝั่ง Angular (Login, Auth Guard, Dashboard Layout, Categories CRUD, Products CRUD พร้อม Pagination & Search) เสร็จสิ้นเรียบร้อย

* **การทดสอบระบบ (Testing):**
  * [x] เขียน Unit Test (xUnit) ฝั่งหลังบ้านครอบคลุม Service Layer เรียบร้อย
