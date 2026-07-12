import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Category {
  id: number;
  name: string;
  description?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:7133/api/categories';

  // ดึงรายการหมวดหมู่ทั้งหมด
  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(this.apiUrl);
  }

  // ดึงหมวดหมู่เดี่ยวตาม ID
  getCategory(id: number): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/${id}`);
  }

  // เพิ่มหมวดหมู่ใหม่
  createCategory(category: { name: string; description?: string }): Observable<Category> {
    return this.http.post<Category>(this.apiUrl, category);
  }

  // แก้ไขหมวดหมู่
  updateCategory(id: number, category: { name: string; description?: string }): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, category);
  }

  // ลบหมวดหมู่
  deleteCategory(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
