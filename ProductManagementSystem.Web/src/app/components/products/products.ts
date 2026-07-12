import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProductService, Product } from '../../services/product.service';
import { CategoryService, Category } from '../../services/category.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './products.html',
  styleUrl: './products.scss'
})
export class Products implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  protected readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  // ข้อมูลและสถานะตารางสินค้า
  protected readonly products = signal<Product[]>([]);
  protected readonly categories = signal<Category[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  // สิทธิ์ระดับผู้ใช้งาน
  protected readonly isAdmin = this.authService.isAdmin;

  // คิวรี่การแบ่งหน้าและฟิลเตอร์ค้นหา
  protected readonly currentPage = signal(1);
  protected readonly totalPages = signal(1);
  protected readonly totalCount = signal(0);
  protected readonly pageSize = signal(10);
  protected readonly searchQuery = signal('');

  // ควบคุมฟรอนต์เอนด์ Modals
  protected readonly isModalOpen = signal(false);
  protected readonly isDeleteConfirmOpen = signal(false);
  protected readonly isDetailsOpen = signal(false);

  // บันทึกการกระทำเป้าหมาย
  protected readonly editingProductId = signal<number | null>(null);
  protected readonly deletingProductId = signal<number | null>(null);
  protected readonly selectedProduct = signal<Product | null>(null);

  // ฟอร์มข้อมูลสินค้าพร้อม Validation เงื่อนไขราคา > 0 และสต็อก >= 0
  protected readonly productForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(1000)]],
    price: [0, [Validators.required, Validators.min(0.01)]],
    stock: [0, [Validators.required, Validators.min(0)]],
    imageUrl: ['', [Validators.maxLength(500)]],
    categoryId: [null as unknown as number, [Validators.required]]
  });

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    
    this.productService.getProducts(
      this.currentPage(), 
      this.pageSize(), 
      this.searchQuery()
    ).subscribe({
      next: (res) => {
        this.products.set(res.items);
        this.totalPages.set(res.totalPages);
        this.totalCount.set(res.totalItems);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load products. Please try again.');
        this.isLoading.set(false);
      }
    });
  }

  loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (data) => this.categories.set(data),
      error: () => console.error('Failed to load categories for dropdown select.')
    });
  }

  onSearch(query: string, event: Event): void {
    event.preventDefault();
    this.searchQuery.set(query.trim());
    this.currentPage.set(1); // รีเซ็ตกลับไปหน้าแรกสุด
    this.loadProducts();
  }

  onClearSearch(input: HTMLInputElement): void {
    input.value = '';
    this.searchQuery.set('');
    this.currentPage.set(1); // รีเซ็ตกลับไปหน้าแรกสุด
    this.loadProducts();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage.set(page);
    this.loadProducts();
  }

  openAddModal(): void {
    this.editingProductId.set(null);
    this.productForm.reset({
      name: '',
      description: '',
      price: 0,
      stock: 0,
      imageUrl: '',
      categoryId: null as unknown as number
    });
    this.isModalOpen.set(true);
  }

  openEditModal(product: Product): void {
    this.editingProductId.set(product.id);
    this.productForm.patchValue({
      name: product.name,
      description: product.description || '',
      price: product.price,
      stock: product.stock,
      imageUrl: product.imageUrl || '',
      categoryId: product.categoryId
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
    this.productForm.reset();
  }

  openDetailsModal(product: Product): void {
    this.selectedProduct.set(product);
    this.isDetailsOpen.set(true);
  }

  closeDetailsModal(): void {
    this.isDetailsOpen.set(false);
    this.selectedProduct.set(null);
  }

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    const formValues = this.productForm.getRawValue();
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const editId = this.editingProductId();
    if (editId !== null) {
      this.productService.updateProduct(editId, formValues).subscribe({
        next: () => {
          this.loadProducts();
          this.closeModal();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Failed to update product.');
          this.isLoading.set(false);
        }
      });
    } else {
      this.productService.createProduct(formValues).subscribe({
        next: () => {
          this.loadProducts();
          this.closeModal();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Failed to create product.');
          this.isLoading.set(false);
        }
      });
    }
  }

  confirmDelete(id: number): void {
    this.deletingProductId.set(id);
    this.isDeleteConfirmOpen.set(true);
  }

  closeDeleteConfirm(): void {
    this.isDeleteConfirmOpen.set(false);
    this.deletingProductId.set(null);
  }

  executeDelete(): void {
    const deleteId = this.deletingProductId();
    if (deleteId === null) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.isDeleteConfirmOpen.set(false);

    this.productService.deleteProduct(deleteId).subscribe({
      next: () => {
        this.loadProducts();
        this.deletingProductId.set(null);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.message || 'Failed to delete product.');
        this.isLoading.set(false);
        this.deletingProductId.set(null);
      }
    });
  }
}
