import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Category } from '../../models/category';
import { CatalogApiService } from '../../services/catalog-api.service';

/**
 * Alta y modificación de un producto sin características de variación: al crear se define su única
 * opción con precio y disponibilidad; al modificar se edita la información comercial del producto.
 */
@Component({
  selector: 'app-product-form',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <h1 class="h3 mb-3">{{ id() ? 'Modificar producto' : 'Nuevo producto' }}</h1>
    @if (error()) {
      <div class="alert alert-danger">{{ error() }}</div>
    }
    <form [formGroup]="form" (ngSubmit)="save()" class="col-md-6">
      <div class="mb-3">
        <label class="form-label" for="name">Nombre</label>
        <input id="name" class="form-control" formControlName="name" />
        @if (form.controls.name.touched && form.controls.name.invalid) {
          <div class="text-danger small">El nombre es obligatorio.</div>
        }
      </div>
      <div class="mb-3">
        <label class="form-label" for="categoryId">Categoría</label>
        <select id="categoryId" class="form-select" formControlName="categoryId">
          <option value="" disabled>Seleccione una categoría</option>
          @for (category of categories(); track category.id) {
            <option [value]="category.id" [disabled]="!category.isActive">
              {{ category.name }}{{ category.isActive ? '' : ' (inactiva)' }}
            </option>
          }
        </select>
      </div>
      <div class="mb-3">
        <label class="form-label" for="description">Descripción (opcional)</label>
        <textarea id="description" class="form-control" rows="2" formControlName="description"></textarea>
      </div>
      <div class="mb-3">
        <label class="form-label" for="imageUrl">Imagen (URL, opcional)</label>
        <input id="imageUrl" class="form-control" formControlName="imageUrl" />
        <div class="form-text">Sin imagen el producto no es visible al público.</div>
      </div>
      @if (!id()) {
        <div class="mb-3">
          <label class="form-label" for="price">Precio</label>
          <input id="price" type="number" min="0" step="0.01" class="form-control" formControlName="price" />
          @if (form.controls.price.touched && form.controls.price.invalid) {
            <div class="text-danger small">Ingrese un precio igual o mayor que 0.</div>
          }
        </div>
        <div class="form-check form-switch mb-3">
          <input id="available" type="checkbox" class="form-check-input" formControlName="isMarkedAvailable" />
          <label class="form-check-label" for="available">Disponible</label>
        </div>
      }
      <button class="btn btn-primary me-2" type="submit" [disabled]="saving()">
        <i class="fa-solid fa-floppy-disk me-1"></i>Guardar
      </button>
      <a class="btn btn-outline-secondary" routerLink="/catalog/products">Cancelar</a>
    </form>
  `,
})
export class ProductForm {
  private readonly api = inject(CatalogApiService);
  private readonly router = inject(Router);

  /** Parámetro de ruta (withComponentInputBinding); ausente al crear. */
  readonly id = input<string>();

  protected readonly categories = signal<Category[]>([]);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.pattern(/\S/)]],
    categoryId: ['', Validators.required],
    description: [''],
    imageUrl: [''],
    price: [0, [Validators.required, Validators.min(0)]],
    isMarkedAvailable: [true],
  });

  constructor() {
    this.api.listCategories(true).subscribe((categories) => this.categories.set(categories));
    queueMicrotask(() => {
      const id = this.id();
      if (id) {
        this.api.getProduct(id).subscribe({
          next: (p) =>
            this.form.patchValue({
              name: p.name,
              categoryId: p.categoryId,
              description: p.description ?? '',
              imageUrl: p.imageUrl ?? '',
            }),
          error: () => this.error.set('No se encontró el producto.'),
        });
      }
    });
  }

  protected save(): void {
    if (this.id()) {
      this.form.controls.price.disable();
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const info = {
      name: v.name,
      description: v.description.trim() || null,
      imageUrl: v.imageUrl.trim() || null,
      categoryId: v.categoryId,
    };
    const id = this.id();
    this.saving.set(true);
    this.error.set(null);
    const request = id
      ? this.api.updateProduct(id, info)
      : this.api.createProduct({
          ...info,
          options: [{ price: v.price, isMarkedAvailable: v.isMarkedAvailable, description: null, imageUrl: null }],
        });
    request.subscribe({
      next: () => this.router.navigate(['/catalog/products']),
      error: (e: HttpErrorResponse) => {
        this.saving.set(false);
        this.error.set(e.error?.detail ?? 'No se pudo guardar el producto.');
      },
    });
  }
}
