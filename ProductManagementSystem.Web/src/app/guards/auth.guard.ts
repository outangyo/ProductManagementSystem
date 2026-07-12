import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // ตรวจสอบความปลอดภัยผ่านตั๋วหลักใน Signal
  if (authService.isAuthenticated()) {
    return true; // ยินยอมให้ผ่านเข้าหน้าเว็บหลัก (เช่น หน้าตารางสินค้า/หมวดหมู่)
  }

  // หากไม่มีตั๋วล็อกอิน ให้สั่งพาเด้งกลับไปที่หน้าล็อกอินหลักทันที
  router.navigate(['/login']);
  return false;
};
