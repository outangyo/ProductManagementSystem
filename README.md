# Product Management System

ระบบจัดการสินค้าและหมวดหมู่สินค้าภายในองค์กร พัฒนาโดยใช้เทคโนโลยี ASP.NET Core 10 Web API และ Angular 22 ร่วมกับฐานข้อมูล SQL Server และระบบความปลอดภัย JWT Authentication

---

## เทคโนโลยีหลัก (Core Technologies)
* **Backend:** ASP.NET Core 10.0 Web API
* **Database Access:** Entity Framework Core 10 (Code-First)
* **Database:** Microsoft SQL Server
* **Frontend:** Angular 22 (Standalone Components, Signals, Reactive Forms)
* **Containerization:** Docker & Docker Compose

---

## ข้อมูลบัญชีผู้ใช้งานเริ่มต้น (Default Accounts)
ระบบได้รับการตั้งค่าข้อมูลบัญชีผู้ใช้งานจำลองสำหรับการทดสอบระดับสิทธิ์ (Role-based Authorization) ดังนี้:

| Role | Username | Password | Permissions |
| --- | --- | --- | --- |
| **Admin** | `admin` | `admin123` | สิทธิ์ระดับผู้ควบคุมระบบ (จัดการข้อมูลสินค้าและหมวดหมู่ทั้งหมด, อัปโหลดรูปภาพ) |
| **User** | `user` | `user123` | สิทธิ์ระดับผู้ใช้งานทั่วไป (เรียกอ่านข้อมูลได้อย่างเดียว - Read-only) |

---

## ขั้นตอนการติดตั้งและรันระบบ (Setup & Deployment)

การทดสอบระบบสามารถทำได้ 2 วิธีหลักดังนี้:

### วิธีที่ 1: การรันผ่าน Docker Compose (แนะนำ)
ต้องการเพียง Docker Desktop ติดตั้งและพร้อมใช้งานในเครื่องโฮสต์

1. เปิด Command Line ในโฟลเดอร์หลักของโปรเจกต์ (Root Directory)
2. รันคำสั่งต่อไปนี้เพื่อทำการ Build และเริ่มต้นบริการทั้งหมด:
   ```bash
   docker compose up --build -d
   ```
3. ระบบจะทำการตั้งค่า Container ฐานข้อมูล, ดำเนินการ Database Migration/Seeding อัตโนมัติ และเปิดบริการในพอร์ตที่กำหนด:
   * **Frontend Web Application:** [http://localhost:4200](http://localhost:4200)
   * **Backend API Swagger Document:** [http://localhost:5040/swagger](http://localhost:5040/swagger)
   * **SQL Server Database Engine:** Port `1433` (Username: `sa`, Password: `SqlPassword123!`)

*หากต้องการหยุดการทำงานของระบบจำลอง ให้รันคำสั่ง `docker compose down`*

---

### วิธีที่ 2: การรันแบบแยกบริการภายในเครื่อง (Local Run)

#### 1. ส่วนฐานข้อมูล (Database Setup)
1. ทำการเชื่อมต่อ SQL Server หรือ LocalDB ในเครื่องของคุณ
2. ตั้งค่า Connection String ในไฟล์ `ProductManagementSystem.API/appsettings.json`
3. ดำเนินการสร้าง Schema ตารางข้อมูลจำลองผ่านคำสั่ง:
   ```bash
   dotnet ef database update --project ProductManagementSystem.Db --startup-project ProductManagementSystem.API
   ```

#### 2. ส่วนบริการหลังบ้าน (Backend API Setup)
1. รันคำสั่งต่อไปนี้เพื่อเปิดการทำงานของ Web API:
   ```bash
   dotnet run --project ProductManagementSystem.API
   ```

#### 3. ส่วนบริการหน้าบ้าน (Frontend Web Setup)
1. เข้าไปยังโฟลเดอร์โครงการหน้าบ้าน:
   ```bash
   cd ProductManagementSystem.Web
   ```
2. ติดตั้ง Dependencies ของระบบ:
   ```bash
   npm install
   ```
3. สั่งรันบริการหน้าเว็บ:
   ```bash
   npm start
   ```
   *(หรือใช้คำสั่ง `ng serve` หากเครื่องของคุณมีการติดตั้ง Angular CLI แบบ Global ไว้แล้ว)*
4. เปิดเบราว์เซอร์เข้าใช้งานทาง [http://localhost:4200](http://localhost:4200)

---

## สรุปฟีเจอร์สำคัญในระบบ (Key Features)
1. **Dynamic Image Upload:** อัปโหลดและจัดการรูปภาพสินค้าโดยเก็บข้อมูลในรูปแบบ Relative Path ในฐานข้อมูล เพื่อป้องกันปัญหา URL เสียหายเมื่อทำการเปลี่ยนพอร์ตหรือสลับสภาวะการรัน (Environment)
2. **Refresh Token Rotation:** ระบบการต่ออายุ Access Token อัตโนมัติเพื่อความปลอดภัยตามมาตรฐาน JWT
3. **Role-based UI Control:** มีระบบตรวจสอบความปลอดภัยและจำกัดสิทธิ์ในระดับหลังบ้าน (API Protection) ควบคู่กับการซ่อนปุ่มดำเนินการ (Create, Edit, Delete) สำหรับผู้ใช้ทั่วไป
4. **Seed Images Integration:** มาพร้อมกับรูปภาพตัวอย่างจริงที่แนบไปพร้อมกับ Git Repository ทำให้ฐานข้อมูลพร้อมแสดงรูปภาพสินค้าทันทีเมื่อติดตั้งใช้งาน
