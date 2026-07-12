import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { Layout } from './components/layout/layout';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: Login },
  {
    path: '',
    component: Layout,
    canActivate: [authGuard],
    children: [
      { 
        path: 'products', 
        loadComponent: () => import('./components/products/products').then(m => m.Products)
      },
      { 
        path: 'categories', 
        loadComponent: () => import('./components/categories/categories').then(m => m.Categories)
      },
      { path: '', redirectTo: 'products', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: 'products' }
];
