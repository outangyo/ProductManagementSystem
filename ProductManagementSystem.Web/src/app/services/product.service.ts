import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  id: number;
  name: string;
  description?: string;
  price: number;
  stock: number;
  imageUrl?: string;
  categoryId: number;
  categoryName?: string;
}

export interface PaginatedProducts {
  items: Product[];
  currentPage: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5040/api/products';

  // ดึงข้อมูลรายการสินค้าแบบแบ่งหน้า ค้นหา และกรอง
  getProducts(page: number = 1, pageSize: number = 10, search: string = ''): Observable<PaginatedProducts> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (search.trim()) {
      params = params.set('search', search);
    }

    return this.http.get<PaginatedProducts>(this.apiUrl, { params });
  }

  // ดึงสินค้าเดี่ยวตาม ID
  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  // สร้างสินค้าใหม่
  createProduct(product: Omit<Product, 'id'>): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, product);
  }

  // แก้ไขสินค้าตาม ID
  updateProduct(id: number, product: Omit<Product, 'id'>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, product);
  }

  // ลบสินค้าตาม ID
  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // อัปโหลดไฟล์รูปภาพสินค้า
  uploadImage(file: File): Observable<{ imageUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ imageUrl: string }>(`${this.apiUrl}/upload`, formData);
  }
}
