import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Category } from '../../models/category';
import { CatalogApiService } from '../../services/catalog-api.service';

@Component({
  selector: 'app-category-list',
  imports: [RouterLink],
  template: `
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h1 class="h3 mb-0">Categorías</h1>
      <a class="btn btn-primary" routerLink="new"><i class="fa-solid fa-plus me-1"></i>Nueva categoría</a>
    </div>
    @if (error()) {
      <div class="alert alert-danger">{{ error() }}</div>
    }
    <table class="table table-hover align-middle">
      <thead>
        <tr><th>Nombre</th><th>Descripción</th><th>Estado</th><th></th></tr>
      </thead>
      <tbody>
        @for (category of categories(); track category.id) {
          <tr>
            <td>{{ category.name }}</td>
            <td>{{ category.description }}</td>
            <td>
              <span class="badge" [class.bg-success]="category.isActive" [class.bg-secondary]="!category.isActive">
                {{ category.isActive ? 'Activa' : 'Inactiva' }}
              </span>
            </td>
            <td class="text-end">
              <a class="btn btn-sm btn-outline-primary me-1" [routerLink]="[category.id, 'edit']">
                <i class="fa-solid fa-pen me-1"></i>Editar
              </a>
              <button type="button" class="btn btn-sm" [class.btn-outline-warning]="category.isActive"
                [class.btn-outline-success]="!category.isActive" (click)="toggle(category)">
                @if (category.isActive) {
                  <i class="fa-solid fa-ban me-1"></i>Dar de baja
                } @else {
                  <i class="fa-solid fa-rotate-left me-1"></i>Reactivar
                }
              </button>
            </td>
          </tr>
        } @empty {
          <tr><td colspan="4" class="text-muted text-center">Aún no hay categorías.</td></tr>
        }
      </tbody>
    </table>
  `,
})
export class CategoryList {
  private readonly api = inject(CatalogApiService);
  protected readonly categories = signal<Category[]>([]);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.api.listCategories(true).subscribe({
      next: (categories) => this.categories.set(categories),
      error: () => this.error.set('No se pudieron cargar las categorías.'),
    });
  }

  protected toggle(category: Category): void {
    const request = category.isActive ? this.api.deactivateCategory(category.id) : this.api.reactivateCategory(category.id);
    request.subscribe({
      next: (updated) => this.categories.update((list) => list.map((c) => (c.id === updated.id ? updated : c))),
      error: () => this.error.set('No se pudo cambiar el estado de la categoría.'),
    });
  }
}
