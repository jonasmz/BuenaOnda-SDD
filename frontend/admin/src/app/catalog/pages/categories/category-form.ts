import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CatalogApiService } from '../../services/catalog-api.service';

/** Alta y modificación de una categoría; sin `id` crea, con `id` modifica. */
@Component({
  selector: 'app-category-form',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <h1 class="h3 mb-3">{{ id() ? 'Modificar categoría' : 'Nueva categoría' }}</h1>
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
        <label class="form-label" for="description">Descripción (opcional)</label>
        <textarea id="description" class="form-control" rows="3" formControlName="description"></textarea>
      </div>
      <button class="btn btn-primary me-2" type="submit" [disabled]="saving()">
        <i class="fa-solid fa-floppy-disk me-1"></i>Guardar
      </button>
      <a class="btn btn-outline-secondary" routerLink="/catalog/categories">Cancelar</a>
    </form>
  `,
})
export class CategoryForm {
  private readonly api = inject(CatalogApiService);
  private readonly router = inject(Router);

  /** Parámetro de ruta (withComponentInputBinding); ausente al crear. */
  readonly id = input<string>();

  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.pattern(/\S/)]],
    description: [''],
  });

  constructor() {
    queueMicrotask(() => {
      const id = this.id();
      if (id) {
        this.api.getCategory(id).subscribe({
          next: (c) => this.form.patchValue({ name: c.name, description: c.description ?? '' }),
          error: () => this.error.set('No se encontró la categoría.'),
        });
      }
    });
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { name, description } = this.form.getRawValue();
    const input = { name, description: description.trim() || null };
    const id = this.id();
    this.saving.set(true);
    this.error.set(null);
    (id ? this.api.updateCategory(id, input) : this.api.createCategory(input)).subscribe({
      next: () => this.router.navigate(['/catalog/categories']),
      error: (e: HttpErrorResponse) => {
        this.saving.set(false);
        this.error.set(e.error?.detail ?? 'No se pudo guardar la categoría.');
      },
    });
  }
}
