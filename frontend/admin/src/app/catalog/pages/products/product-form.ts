import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Characteristic, Product, SellableOption } from '../../models/product';
import { Category } from '../../models/category';
import { CatalogApiService } from '../../services/catalog-api.service';
import { OptionForm } from '../../components/option-form/option-form';
import { emptyVariation, VariationDraft, VariationEditor } from '../../components/variation-editor/variation-editor';

/**
 * Alta y modificación de un producto: al crear se define su estructura de variación (características y
 * opciones con precio); al modificar se edita la información comercial y se consultan sus opciones.
 */
@Component({
  selector: 'app-product-form',
  imports: [ReactiveFormsModule, RouterLink, VariationEditor, OptionForm],
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
        <app-variation-editor [(draft)]="variation" />
      } @else {
        <h2 class="h5">Opciones</h2>
        @for (option of options(); track option.id) {
          <app-option-form [productId]="id()!" [option]="option" [characteristics]="characteristics()"
            (productChanged)="applyProduct($event)" (removed)="reload()" />
        }
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
  protected readonly characteristics = signal<Characteristic[]>([]);
  protected readonly options = signal<SellableOption[]>([]);
  protected readonly variation = signal<VariationDraft>(emptyVariation());
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.pattern(/\S/)]],
    categoryId: ['', Validators.required],
    description: [''],
    imageUrl: [''],
  });

  constructor() {
    this.api.listCategories(true).subscribe((categories) => this.categories.set(categories));
    queueMicrotask(() => this.reload());
  }

  protected reload(): void {
    const id = this.id();
    if (id) {
      this.api.getProduct(id).subscribe({
        next: (p) => {
          this.form.patchValue({
            name: p.name,
            categoryId: p.categoryId,
            description: p.description ?? '',
            imageUrl: p.imageUrl ?? '',
          });
          this.applyProduct(p);
        },
        error: () => this.error.set('No se encontró el producto.'),
      });
    }
  }

  protected applyProduct(product: Product): void {
    this.characteristics.set(product.characteristics);
    this.options.set(product.options);
  }

  protected save(): void {
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
    const draft = this.variation();
    if (!id && draft.options.some((o) => !Number.isFinite(o.price) || o.price < 0)) {
      this.error.set('Cada opción necesita un precio igual o mayor que 0.');
      return;
    }
    this.saving.set(true);
    this.error.set(null);
    const request = id
      ? this.api.updateProduct(id, info)
      : this.api.createProduct({
          ...info,
          characteristics: draft.characteristics,
          options: draft.options.map((o) => ({ ...o, description: null, imageUrl: null })),
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
