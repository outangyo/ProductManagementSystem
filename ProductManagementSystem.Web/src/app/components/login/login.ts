import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // สัญญาณบอกสถานะการหมุนโหลดและคำสั่งแจ้งเตือนความผิดพลาด
  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  // ตั้งค่าฟอร์มพร้อมกฎดักข้อมูลห้ามเป็นค่าว่าง
  protected readonly loginForm = this.fb.nonNullable.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched(); // ไฮไลท์สีแดงช่องที่ลืมกรอก
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const credentials = this.loginForm.getRawValue();

    this.authService.login(credentials).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/products']); // ไปหน้าตารางสินค้าทันที
      },
      error: (err) => {
        this.isLoading.set(false);
        // ดึงข้อความภาษาไทยที่เรายำดักไว้จากหลังบ้านมาโชว์ตรง ๆ
        this.errorMessage.set(err.error?.message || 'การเชื่อมต่อเซิร์ฟเวอร์ขัดข้อง กรุณาลองใหม่อีกครั้ง');
      }
    });
  }
}
