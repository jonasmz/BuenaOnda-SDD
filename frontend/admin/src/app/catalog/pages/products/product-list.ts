import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Category } from '../../models/category';
import { Product } from '../../models/product';
import { CatalogApiService } from '../../services/catalog-api.service';

@Component({
  selector: 'app-product-list',
  imports: [RouterLink, CurrencyPipe],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h1 class="h3 mb-0">Productos</h1>
      <a class="btn btn-primary" routerLink="new"><i class="fa-solid fa-plus me-1"></i>Nuevo producto</a>
    </div>
    @if (error()) {
      <div class="alert alert-danger">{{ error() }}</div>
    }
    <table class="table table-hover align-middle">
      <thead>
        <tr><th>Nombre</th><th>Categoría</th><th>Precio</th><th>Estado</th><th></th></tr>
      </thead>
      <tbody>
        @for (product of products(); track product.id) {
          <tr>
            <td>{{ product.name }}</td>
            <td>{{ categoryName(product.categoryId) }}</td>
            <td>
              @for (option of product.options; track option.id) {
                <div>{{ option.price | currency }}</div>
              }
            </td>
            <td>
              <span class="badge me-1" [class.bg-success]="product.isAvailable" [class.bg-warning]="!product.isAvailable">
                {{ product.isAvailable ? 'Disponible' : 'No disponible' }}
              </span>
              <span class="badge me-1" [class.bg-info]="product.isVisibleToPublic" [class.bg-secondary]="!product.isVisibleToPublic">
                {{ product.isVisibleToPublic ? 'Visible al público' : 'No visible' }}
              </span>
              @if (!product.hasImage) {
                <span class="badge bg-light text-dark"><i class="fa-regular fa-image me-1"></i>sin imagen</span>
              }
              @if (!product.isActive) {
                <span class="badge bg-secondary">Inactivo</span>
              }
            </td>
            <td class="text-end">
              <a class="btn btn-sm btn-outline-primary" [routerLink]="[product.id, 'edit']">
                <i class="fa-solid fa-pen me-1"></i>Editar
              </a>
            </td>
          </tr>
        } @empty {
          <tr><td colspan="5" class="text-muted text-center">Aún no hay productos.</td></tr>
        }
      </tbody>
    </table>
  `,
})
export class ProductList {
  private readonly api = inject(CatalogApiService);
  protected readonly products = signal<Product[]>([]);
  protected readonly categories = signal<Category[]>([]);
  protected readonly error = signal<string | null>(null);
  private readonly names = computed(() => new Map(this.categories().map((c) => [c.id, c.name])));

  constructor() {
    this.api.listProducts(undefined, true).subscribe({
      next: (products) => this.products.set(products),
      error: () => this.error.set('No se pudieron cargar los productos.'),
    });
    this.api.listCategories(true).subscribe((categories) => this.categories.set(categories));
  }

  protected categoryName(id: string): string {
    return this.names().get(id) ?? '';
  }
}
