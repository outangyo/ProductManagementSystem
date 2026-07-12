import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './layout.html',
  styleUrl: './layout.scss'
})
export class Layout {
  protected readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  // ดึงชื่อผู้ใช้มาแสดงบน Header
  protected readonly userFullName = this.authService.getUserFullName();
  protected readonly username = this.authService.getUsername();

  // สถานะการเปิด/ปิดกล่องยืนยันการออกจากระบบ
  protected readonly isLogoutConfirmOpen = signal(false);

  openLogoutConfirm(): void {
    this.isLogoutConfirmOpen.set(true);
  }

  closeLogoutConfirm(): void {
    this.isLogoutConfirmOpen.set(false);
  }

  executeLogout(): void {
    this.isLogoutConfirmOpen.set(false);
    this.authService.logout();
  }
}
