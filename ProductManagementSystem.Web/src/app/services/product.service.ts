import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  id: number;
  name: string;
  price: number;
  stock: number;
  imageUrl?: string;
  categoryId: number;
  categoryName?: string;
}

export interface PaginatedProducts {
  items: Product[];
  page: number;
  limit: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:7133/api/products';

  // ดึงข้อมูลรายการสินค้าแบบแบ่งหน้า ค้นหา และกรอง
  getProducts(page: number = 1, limit: number = 5, search: string = ''): Observable<PaginatedProducts> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('limit', limit.toString());

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
}
