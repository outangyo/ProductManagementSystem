import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.currentToken();

  // 1. แนบ Access Token เข้าไปใน Authorization Header เสมอ (ยกเว้นเรียก API ล็อกอินหรือต่ออายุตั๋วเอง)
  let authReq = req;
  if (token && !req.url.includes('/api/auth/login') && !req.url.includes('/api/auth/refresh')) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  // 2. ส่ง Request ต่อไป พร้อมดักจับเออร์เรอร์ 401 (Unauthorized) เพื่อสลับทำระบบต่อตั๋วอัตโนมัติ
  return next(authReq).pipe(
    catchError((error) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !req.url.includes('/api/auth/login') &&
        !req.url.includes('/api/auth/refresh')
      ) {
        // เมื่อได้รับ 401 ให้ทำการขอต่อตั๋วใบใหม่ผ่านระบบหลังบ้าน (Refresh Token Flow)
        return authService.refreshToken().pipe(
          switchMap((response) => {
            // เมื่อได้ตั๋วหลักใบใหม่มาแล้ว ให้โคลน Request เดิมแล้วยัดตั๋วใหม่เข้าไปยิงใหม่อีกรอบทันที!
            const newAuthReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${response.token}`
              }
            });
            return next(newAuthReq);
          }),
          catchError((refreshError) => {
            // หากตั๋วต่อตั๋วก็พัง (เช่น 7 วันหมดอายุแล้ว) ให้ดีดผู้ใช้ออกไปล็อกอินใหม่ทันที
            authService.logout();
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
