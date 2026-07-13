# Product Management System (Mini Project)

ระบบจัดการสินค้าและหมวดหมู่สินค้าภายในองค์กร พัฒนาด้วย **C# .NET 10 Web API** และ **Angular v22 (Standalone Components)** พร้อมระบบความปลอดภัย **JWT Authentication** (มีระบบ Refresh Token Rotation & Role-based Authorization) และฐานข้อมูล **SQL Server**

---

## 🛠️ เทคโนโลยีที่ใช้ (Tech Stack)
* **Backend:** ASP.NET Core Web API (.NET 10.0)
* **Database Access:** Entity Framework Core (Code First) + SQL Server
* **Frontend:** Angular v22 (Standalone Components, Signals, Reactive Forms)
* **CSS & Styling:** CSS variables (Custom Theme) + Responsive Design
* **Containerization:** Docker & Docker Compose

---

## 🔑 บัญชีผู้ใช้งานเริ่มต้น (Seed Accounts)
ระบบมีการเพิ่มบัญชีผู้ใช้เริ่มต้นเข้าฐานข้อมูลให้โดยอัตโนมัติเพื่อให้สามารถเข้าตรวจสอบระบบได้ทันที:

| บทบาท (Role) | ชื่อผู้ใช้ (Username) | รหัสผ่าน (Password) | สิทธิ์การใช้งาน (Permissions) |
| --- | --- | --- | --- |
| **Admin** | `admin` | `admin123` | จัดการข้อมูลได้ทั้งหมด (สร้าง, แก้ไข, ลบ, อัปโหลดรูปภาพ) |
| **User** | `user` | `user123` | อ่านข้อมูลได้อย่างเดียว (Read-only) ไม่สามารถเพิ่ม/แก้ไข/ลบได้ |

---

## 🚀 ขั้นตอนการติดตั้งและรันระบบ (How to Run)

คุณสามารถเลือกรันระบบได้ 2 รูปแบบตามความสะดวกดังนี้:

### รูปแบบที่ 1: รันผ่าน Docker (แนะนำ - ง่ายและรวดเร็วที่สุด 🐳)
**ความต้องการ:** ต้องติดตั้งโปรแกรม [Docker Desktop](https://www.docker.com/products/docker-desktop/) และเปิดโปรแกรมให้พร้อมทำงาน

1. เปิด Terminal ในโฟลเดอร์หลักของโปรเจกต์ (Root Directory)
2. รันคำสั่งเปิดตู้คอนเทนเนอร์ทั้งหมด:
   ```bash
   docker compose up --build -d
   ```
3. ระบบจะดาวน์โหลดข้อมูล, คอมไพล์ตัวโปรแกรม, สร้างตารางฐานข้อมูลพร้อมข้อมูลเริ่มต้น (Seed Data), และจำลองระบบรันขึ้นมาให้โดยอัตโนมัติในเบื้องหลัง
4. **ลิงก์สำหรับเข้าตรวจสอบใช้งาน:**
   * **หน้าบ้าน (Frontend Web):** [http://localhost:4200](http://localhost:4200)
   * **หลังบ้าน (API Swagger):** [http://localhost:5040/swagger](http://localhost:5040/swagger)
   * **ฐานข้อมูล (SQL Server):** Port `1433` (Username: `sa`, Password: `SqlPassword123!`)

*(หากต้องการปิดตู้คอนเทนเนอร์ทั้งหมด ให้รันคำสั่ง `docker compose down`)*

---

### รูปแบบที่ 2: รันแบบปกติทีละส่วน (Local Run)

#### 1. ฝั่งฐานข้อมูล (Database)
1. ติดตั้ง SQL Server หรือ LocalDB ในเครื่องของคุณ
2. เข้าไปตั้งค่า Connection String ในไฟล์ [ProductManagementSystem.API/appsettings.json](file:///C:/Project/ProductManagementSystem/ProductManagementSystem.API/appsettings.json) ให้เชื่อมต่อไปยังเซิร์ฟเวอร์ของคุณได้ถูกต้อง
3. สั่งสร้างตารางฐานข้อมูลและอัปเดตโมเดลล่าสุดด้วยคำสั่ง:
   ```bash
   dotnet ef database update --project ProductManagementSystem.Db --startup-project ProductManagementSystem.API
   ```

#### 2. ฝั่งหลังบ้าน (Backend API)
1. ตรวจสอบให้แน่ใจว่าพอร์ต `5040` ในเครื่องไม่ได้เปิดใช้งานซ้ำซ้อน
2. สั่งรันโครงการ API ด้วยคำสั่ง:
   ```bash
   dotnet run --project ProductManagementSystem.API
   ```

#### 3. ฝั่งหน้าบ้าน (Frontend Angular Web)
1. เปิด Terminal เข้าไปที่โฟลเดอร์หน้าบ้าน:
   ```bash
   cd ProductManagementSystem.Web
   ```
2. ติดตั้ง Library และ Dependencies ที่จำเป็น:
   ```bash
   npm install
   ```
3. สั่งรันหน้าเว็บ:
   ```bash
   npm start
   ```
4. เปิดเบราว์เซอร์เข้าไปที่: [http://localhost:4200](http://localhost:4200)

---

## 🌟 ฟีเจอร์ที่พัฒนาสำเร็จในระบบ (Key Features)
1. **ระบบอัปโหลดรูปภาพสินค้าถาวร (Dynamic Product Image Upload):**
   * เก็บชื่อพาธแบบสัมพัทธ์ (Relative path) ในฐานข้อมูล และแมป URL เต็มรูปแบบตาม Host/Scheme ที่ใช้งานปัจจุบันโดยอัตโนมัติ ป้องกันปัญหารูปภาพขาดการแสดงผลเมื่อเปลี่ยนพอร์ตหรือย้ายขึ้นระบบคลาวด์/คอนเทนเนอร์
   * สินค้าเริ่มต้น (Seed Data) มีรูปภาพจำลองประกอบสวยงามติดตั้งไปพร้อมกับโปรเจกต์ในระบบ Git ทันที
2. **ระบบต่ออายุตั๋วอัตโนมัติ (Refresh Token Rotation):** รองรับการแอบขอ Access Token ใบใหม่เมื่อหมดอายุ เพื่อความปลอดภัยระดับมาตรฐานสากล
3. **การรักษาความปลอดภัยตามบทบาท (Role-based Authorization):** ซ่อนเมนูการลบ/เพิ่ม/แก้ไขสำหรับบัญชีผู้ใช้ทั่วไป และจำกัดสิทธิ์ในระดับหลังบ้าน (API Protection)
4. **Product CRUD & Category CRUD:** ระบบค้นหาสินค้า คัดกรองหมวดหมู่ และการแบ่งหน้าแสดงผลสินค้า (Pagination)
