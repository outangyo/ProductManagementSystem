# Workspace Instructions

โปรดปฏิบัติตามข้อกำหนดและกรอบงานของโปรเจคนี้อย่างเคร่งครัด
ข้อ 1-4 ห้ามเเก้ไขเด็ดขาด

## 1. โจทย์ (Scenario)
* บริษัทต้องการระบบจัดการสินค้าภายใน ให้ผู้ใช้ที่ login แล้วสามารถบริหารจัดการรายการสินค้าและหมวดหมู่สินค้าได้ ผู้สมัครต้องพัฒนาทั้งฝั่ง Frontend (Angular) และ Backend (.NET Web API) พร้อมระบบ Authentication ด้วย JWT

## 2. Functional Requirements
### 2.1 Authentication (JWT)
* หน้า Login รับ username / password
* Mock user ได้ (hardcode 1–2 accounts หรือ seed ใน database)
* ออก JWT token หลัง login สำ เร็จ พร้อม expiration time
* ทุก API ของ Product / Category ต้องผ่าน JWT authentication
* ถ้า token หมดอายุ หรือไม่มี token ให้ redirect กลับหน้า login
* มีปุ่ม Logout ที่ใช้งานได้

### 2.2 Category Management (CRUD แบบง่าย)
* แสดงรายการ category ทั้งหมด
* เพิ่ม category ใหม่ (Name, Description)
* แก้ไข / ลบ category

### 2.3 Product Management (CRUD หลัก)
* แสดงรายการสินค้าแบบ table มี pagination (เช่น 10 รายการต่อหน้า)
* มีช่อง search ตามชื่อสินค้า
* เพิ่มสินค้าใหม่: Name, Description, Price, Stock, Category (dropdown จาก category ที่มี)
* แก้ไขสินค้า
* ลบสินค้า (ต้องมี confirm dialog)
* ดูรายละเอียดสินค้า (แยกหน้าหรือใช้ modal ก็ได้)

### 2.4 Validation
* Validate ทั้งฝั่ง client และ server
* เงื่อนไข: Price > 0, Stock ≥ 0, Name ไม่ว่าง ฯลฯ
* แสดง error message ที่ผู้ใช้เข้าใจได้

## 3. Technical Requirements
### 3.1 Backend (C# .NET)
* .NET 10
* ASP.NET Core Web API
* Entity Framework Core (Code First) + SQL Server
* JWT Bearer Authentication
* Layered architecture พื้นฐาน (Controller / Service / DbContext)
* แยก DTO ออกจาก Entity
* Return HTTP status code ที่เหมาะสม (200, 201, 400, 401, 404 ฯลฯ)

### 3.2 Frontend (Angular)
* Angular v21 (standalone components หรือ NgModule ก็ได้)
* Routing อย่างน้อย: /login, /products, /categories
* Route guard สำ หรับหน้าที่ต้อง login
* HTTP Interceptor สำ หรับแนบ JWT token อัตโนมัติ
* Reactive Forms (ใช้ Template-driven ได้ แต่ Reactive จะได้คะแนนดีกว่า)
* UI library ใช้อะไรก็ได้ (Angular Material, PrimeNG, Bootstrap หรือเขียนเอง) — แต่หน้าตาต้องดูใช้งานได้จริง

### 3.3 Deliverables
* Source code ใน Git repository พร้อม commit history ที่อ่านเข้าใจได้
* README บอกวิธี setup และ run โครงการ (ต้องการอะไรบ้าง, กี่ command)
* DB migration หรือ seed script

## 4. Advance Requirement
* [x] Unit test ฝั่ง backend (xUnit / NUnit) อย่างน้อย service layer [เสร็จสิ้น]
* [x] Upload รูปสินค้า [เสร็จสิ้น]
* [x] Dockerize ทั้ง backend และ frontend [เสร็จสิ้น]
* [x] Refresh token mechanism [เสร็จสิ้น]
* [x] Role-based authorization (เช่น Admin ลบได้ แต่ User ดูได้อย่างเดียว) [เสร็จสิ้น]

## 6. สถานะการทำงานปัจจุบัน (Current Progress Tracking)
* **การติดตามงานล่าสุด:**
  * สามารถดูเป้าหมายปัจจุบันและประวัติงานที่ทำเสร็จแล้วทั้งหมดได้ที่ [progress-track.md](file:///C:/Project/ProductManagementSystem/.agents/rules/progress-track.md) เพื่อความสะอาดและประหยัด Token ในแชท (โดยเป้าหมายปัจจุบัน ณ ตอนนี้คือการทำ **Dockerize**)

## 5. กติกาและแนวทางการพัฒนา (Development Rules)
* **การใช้สิทธิ์และการรันคำสั่ง (Permission & Command Execution):** AI ควรวางแผนการรันคำสั่งต่าง ๆ หรือการแก้ไขไฟล์โดยการรวบรวมทำพร้อมกันเป็นรอบ ๆ (Batching/Group operations) เพื่อลดภาระและไม่ให้ผู้พัฒนา (Developer) ต้องคอยกดยืนยัน (Approve/Enter) บ่อย ๆ ในระบบความปลอดภัยของ Antigravity
* **การแลกเปลี่ยนและโต้แย้งทางเทคนิค (Active Collaboration & Feedback):** เมื่อผู้พัฒนา (Developer) สั่งงาน หรือให้ทำในสิ่งที่มีความผิดพลาด มีช่องโหว่ หรือมีจุดที่น่าสงสัยสังเกตได้ AI จะต้องทักท้วง แนะนำ และร่วมพูดคุยแลกเปลี่ยนความเห็นเพื่อหาวิธีที่ดีที่สุดร่วมกัน ไม่ทำตามคำสั่งโดยไร้ข้อสงสัย (No blind obedience) เพื่อให้เกิดการพูดคุยและการเรียนรู้ร่วมกันในการพัฒนาโค้ด
* **ข้อบังคับพิเศษสำหรับการทำ Git Commit & Push (Strict Git Rules):** ทุกครั้งที่จะทำการ Commit หรือ Push โค้ด **AI ต้องถามและขออนุญาตก่อนเสมอ** โดยเมื่อเขียนโค้ดเสร็จแล้ว AI จะต้องแสดงชุดคำสั่ง (Git Command) และข้อความอธิบาย (Commit Message) ให้ผู้พัฒนาอ่าน ตรวจทาน และอนุมัติก่อนลงมือทำจริง **ห้ามทำการ Commit หรือ Push โดยอัตโนมัติหรือไม่มีการถามก่อนโดยเด็ดขาด**
