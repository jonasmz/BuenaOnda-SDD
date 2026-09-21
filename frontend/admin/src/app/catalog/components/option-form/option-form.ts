import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Characteristic, Product, SellableOption } from '../../models/product';
import { CatalogApiService } from '../../services/catalog-api.service';

/**
 * Edición de una opción existente: precio, valores, descripción e imagen, conmutador de disponibilidad
 * manual y acciones de dar de baja, reactivar y eliminar (FR-012, FR-014, FR-015, FR-021).
 * Cada acción devuelve el producto actualizado por el evento `productChanged`.
 */
@Component({
  selector: 'app-option-form',
  imports: [ReactiveFormsModule],
  template: `
    <form [formGroup]="form" (ngSubmit)="save()" class="border rounded p-3 mb-3" [class.opacity-50]="!option().isActive">
      <div class="d-flex justify-content-between align-items-center mb-2">
        <strong>{{ title() }}</strong>
        <span class="badge" [class.bg-success]="option().isAvailable" [class.bg-warning]="!option().isAvailable">
          {{ option().isAvailable ? 'Disponible' : 'No disponible' }}
        </span>
      </div>
      @if (error()) {
        <div class="alert alert-danger py-1" role="alert">
          {{ error() }}
          @if (conflictOnDelete()) {
            <button type="button" class="btn btn-sm btn-outline-warning ms-2" (click)="deactivate()">Dar de baja</button>
          }
        </div>
      }
      <div class="row g-2">
        @for (characteristic of characteristics(); track characteristic.id) {
          <div class="col-md-3">
            <label class="form-label small">{{ characteristic.name }}</label>
            <input class="form-control form-control-sm" [formControlName]="'v_' + characteristic.id" />
          </div>
        }
        <div class="col-md-2">
          <label class="form-label small">Precio</label>
          <input type="number" min="0" step="0.01" class="form-control form-control-sm" formControlName="price" />
        </div>
        <div class="col-md-4">
          <label class="form-label small">Descripción</label>
          <input class="form-control form-control-sm" formControlName="description" />
        </div>
        <div class="col-md-4">
          <label class="form-label small">Imagen (URL)</label>
          <input class="form-control form-control-sm" formControlName="imageUrl" />
        </div>
      </div>
      <div class="d-flex flex-wrap gap-2 align-items-center mt-3">
        <div class="form-check form-switch me-2">
          <input id="avail-{{ option().id }}" type="checkbox" class="form-check-input" role="switch"
            [checked]="option().isMarkedAvailable" (change)="setAvailability($any($event.target).checked)" />
          <label class="form-check-label" for="avail-{{ option().id }}">Disponible</label>
        </div>
        <button class="btn btn-sm btn-primary" type="submit" [disabled]="form.invalid">
          <i class="fa-solid fa-floppy-disk me-1"></i>Guardar opción
        </button>
        @if (option().isActive) {
          <button type="button" class="btn btn-sm btn-outline-warning" (click)="deactivate()">
            <i class="fa-solid fa-ban me-1"></i>Dar de baja
          </button>
        } @else {
          <button type="button" class="btn btn-sm btn-outline-success" (click)="reactivate()">
            <i class="fa-solid fa-rotate-left me-1"></i>Reactivar
          </button>
        }
        <button type="button" class="btn btn-sm btn-outline-danger" (click)="remove()">
          <i class="fa-solid fa-trash me-1"></i>Eliminar
        </button>
      </div>
    </form>
  `,
})
export class OptionForm {
  private readonly api = inject(CatalogApiService);

  readonly productId = input.required<string>();
  readonly option = input.required<SellableOption>();
  readonly characteristics = input<Characteristic[]>([]);
  readonly productChanged = output<Product>();
  /** Se emite tras eliminar la opción; el producto debe recargarse. */
  readonly removed = output<void>();

  protected readonly error = signal<string | null>(null);
  protected readonly conflictOnDelete = signal(false);
  /** Controles fijos más un control `v_<id>` por cada característica del producto. */
  protected readonly form = new FormGroup<Record<string, AbstractControl>>({
    price: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
    description: new FormControl('', { nonNullable: true }),
    imageUrl: new FormControl('', { nonNullable: true }),
  });
  protected readonly title = computed(() => {
    const values = this.characteristics().map((c) => this.option().values[c.name]).filter(Boolean);
    return values.length > 0 ? values.join(' · ') : 'Opción única';
  });

  constructor() {
    effect(() => {
      const option = this.option();
      for (const characteristic of this.characteristics()) {
        const name = `v_${characteristic.id}`;
        if (!this.form.contains(name)) {
          this.form.addControl(name, this.newValueControl());
        }
        this.form.get(name)?.setValue(option.values[characteristic.name] ?? '');
      }
      this.form.patchValue({
        price: option.price,
        description: option.description ?? '',
        imageUrl: option.imageUrl ?? '',
      });
    });
  }

  private newValueControl() {
    return new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.pattern(/\S/)] });
  }

  protected save(): void {
    if (this.form.invalid) {
      return;
    }
    const raw = this.form.getRawValue() as Record<string, string | number>;
    const values = Object.fromEntries(
      this.characteristics().map((c) => [c.name, String(raw[`v_${c.id}`] ?? '')]),
    );
    this.run(
      this.api.updateOption(this.productId(), this.option().id, {
        values,
        price: Number(raw['price']),
        description: String(raw['description'] ?? '').trim() || null,
        imageUrl: String(raw['imageUrl'] ?? '').trim() || null,
      }),
    );
  }

  protected setAvailability(isMarkedAvailable: boolean): void {
    this.run(this.api.setOptionAvailability(this.productId(), this.option().id, isMarkedAvailable));
  }

  protected deactivate(): void {
    this.run(this.api.deactivateOption(this.productId(), this.option().id));
  }

  protected reactivate(): void {
    this.run(this.api.reactivateOption(this.productId(), this.option().id));
  }

  protected remove(): void {
    this.error.set(null);
    this.conflictOnDelete.set(false);
    this.api.deleteOption(this.productId(), this.option().id).subscribe({
      next: () => this.removed.emit(),
      error: (e: HttpErrorResponse) => {
        this.error.set(e.error?.detail ?? 'No se pudo eliminar la opción.');
        // 409: opción referenciada o última del producto; se ofrece la baja como alternativa.
        this.conflictOnDelete.set(e.status === 409 && this.option().isActive);
      },
    });
  }

  private run(request: ReturnType<CatalogApiService['deactivateOption']>): void {
    this.error.set(null);
    this.conflictOnDelete.set(false);
    request.subscribe({
      next: (product) => this.productChanged.emit(product),
      error: (e: HttpErrorResponse) => this.error.set(e.error?.detail ?? 'No se pudo guardar la opción.'),
    });
  }
}
