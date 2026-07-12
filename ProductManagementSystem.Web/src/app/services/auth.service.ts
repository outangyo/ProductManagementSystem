import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiUrl = 'https://localhost:7133/api/auth';

  // ใช้ Signal เก็บค่า Token สำหรับการตรวจสอบสิทธิ์แบบเรียลไทม์
  private readonly tokenSignal = signal<string | null>(localStorage.getItem('token'));

  // ดึงค่าสิทธิ์แบบอ่านอย่างเดียวด้วย Computed Signals
  readonly isAuthenticated = computed(() => !!this.tokenSignal());
  readonly currentToken = computed(() => this.tokenSignal());
  readonly isAdmin = computed(() => {
    const token = this.tokenSignal();
    if (!token) return false;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'];
      return role === 'Admin';
    } catch {
      return false;
    }
  });

  // ฟังก์ชันล็อกอินเข้าสู่ระบบ
  login(credentials: { username: string; password: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        if (response && response.token) {
          // บันทึกตั๋วหลัก ตั๋วต่ออายุ และข้อมูลผู้ใช้ลงบราวเซอร์
          localStorage.setItem('token', response.token);
          localStorage.setItem('refreshToken', response.refreshToken);
          localStorage.setItem('username', response.username);
          localStorage.setItem('fullName', response.fullName);
          
          // อัปเดตตั๋วหลักในหน่วยความจำ Signal
          this.tokenSignal.set(response.token);
        }
      })
    );
  }

  // ฟังก์ชันขอตั๋ว Access Token ใบใหม่ (Refresh Token Flow)
  refreshToken(): Observable<any> {
    const accessToken = localStorage.getItem('token') || '';
    const refreshToken = localStorage.getItem('refreshToken') || '';

    return this.http.post<any>(`${this.apiUrl}/refresh`, { accessToken, refreshToken }).pipe(
      tap(response => {
        if (response && response.token) {
          // บันทึกตั๋วหลักใบใหม่ และตั๋วต่ออายุใบใหม่ที่สับเปลี่ยนมา
          localStorage.setItem('token', response.token);
          localStorage.setItem('refreshToken', response.refreshToken);
          
          // อัปเดตตั๋วหลักในหน่วยความจำ Signal
          this.tokenSignal.set(response.token);
        }
      })
    );
  }

  // ฟังก์ชันออกจากระบบ
  logout(): void {
    // ล้างข้อมูลความปลอดภัยในบราวเซอร์ทั้งหมด
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('username');
    localStorage.removeItem('fullName');

    // รีเซ็ตค่าใน Signal
    this.tokenSignal.set(null);

    // เด้งกลับหน้าจอล็อกอิน
    this.router.navigate(['/login']);
  }

  // ดึงชื่อจริงผู้ล็อกอินมาโชว์บนบอร์ด
  getUserFullName(): string {
    return localStorage.getItem('fullName') || 'User';
  }

  // ดึงข้อมูลชื่อผู้ใช้
  getUsername(): string {
    return localStorage.getItem('username') || '';
  }
}
