import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoryService, Category } from '../../services/category.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './categories.html',
  styleUrl: './categories.scss'
})
export class Categories implements OnInit {
  private readonly categoryService = inject(CategoryService);
  protected readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  // สัญญาณเก็บข้อมูลรายการหมวดหมู่
  protected readonly categories = signal<Category[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  // สิทธิ์ของผู้นำทาง (Admin / User)
  protected readonly isAdmin = this.authService.isAdmin;

  // ควบคุมการแสดงผล Modal
  protected readonly isModalOpen = signal(false);
  protected readonly isDeleteConfirmOpen = signal(false);
  
  // เก็บเป้าหมายของการแก้ไข / ลบ
  protected readonly editingCategoryId = signal<number | null>(null);
  protected readonly deletingCategoryId = signal<number | null>(null);

  // ฟอร์มสำหรับเพิ่ม/แก้ไขหมวดหมู่
  protected readonly categoryForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]]
  });

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.categoryService.getCategories().subscribe({
      next: (data) => {
        this.categories.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load categories. Please try again.');
        this.isLoading.set(false);
      }
    });
  }

  openAddModal(): void {
    this.editingCategoryId.set(null);
    this.categoryForm.reset();
    this.isModalOpen.set(true);
  }

  openEditModal(category: Category): void {
    this.editingCategoryId.set(category.id);
    this.categoryForm.patchValue({
      name: category.name,
      description: category.description || ''
    });
    this.isModalOpen.set(true);
  }

  closeModal(): void {
    this.isModalOpen.set(false);
    this.categoryForm.reset();
  }

  onSubmit(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    const formValues = this.categoryForm.getRawValue();
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const editId = this.editingCategoryId();
    if (editId !== null) {
      // โหมดแก้ไข (Update)
      this.categoryService.updateCategory(editId, formValues).subscribe({
        next: () => {
          this.loadCategories();
          this.closeModal();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Failed to update category.');
          this.isLoading.set(false);
        }
      });
    } else {
      // โหมดเพิ่มใหม่ (Create)
      this.categoryService.createCategory(formValues).subscribe({
        next: () => {
          this.loadCategories();
          this.closeModal();
        },
        error: (err) => {
          this.errorMessage.set(err.error?.message || 'Failed to create category.');
          this.isLoading.set(false);
        }
      });
    }
  }

  confirmDelete(id: number): void {
    this.deletingCategoryId.set(id);
    this.isDeleteConfirmOpen.set(true);
  }

  closeDeleteConfirm(): void {
    this.isDeleteConfirmOpen.set(false);
    this.deletingCategoryId.set(null);
  }

  executeDelete(): void {
    const deleteId = this.deletingCategoryId();
    if (deleteId === null) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.isDeleteConfirmOpen.set(false);

    this.categoryService.deleteCategory(deleteId).subscribe({
      next: () => {
        this.loadCategories();
        this.deletingCategoryId.set(null);
      },
      error: (err) => {
        // ดักจับฟีดแบกเออร์เรอร์ภาษาไทยจากหลังบ้าน (เช่น ลบหมวดหมู่ที่มีสินค้าผูกไว้ไม่ได้)
        this.errorMessage.set(err.error?.message || 'Failed to delete category.');
        this.isLoading.set(false);
        this.deletingCategoryId.set(null);
      }
    });
  }
}
