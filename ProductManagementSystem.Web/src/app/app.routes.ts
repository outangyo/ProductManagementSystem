import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  // หน้ารายการสินค้า และหมวดหมู่จะถูกปกป้องด้วย authGuard
  { 
    path: 'products', 
    loadComponent: () => import('./components/products/products').then(m => m.Products),
    canActivate: [authGuard] 
  },
  { 
    path: 'categories', 
    loadComponent: () => import('./components/categories/categories').then(m => m.Categories),
    canActivate: [authGuard] 
  },
  // เส้นทางเริ่มต้น หากไม่ระบุให้วิ่งไปหน้าแสดงสินค้า (ซึ่งจะเด้งไปล็อกอินหากยังไม่มีตั๋ว)
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  // เส้นทางมั่ว ๆ ให้ตีกลับไปหน้าสินค้า
  { path: '**', redirectTo: 'products' }
];
