import { Component, computed, model } from '@angular/core';

/** Opción en edición: un valor de texto por cada característica, más precio y disponibilidad. */
export interface OptionDraft {
  values: Record<string, string>;
  price: number;
  isMarkedAvailable: boolean;
}

/** Estructura de variación en edición: características dinámicas y matriz de opciones. */
export interface VariationDraft {
  characteristics: string[];
  options: OptionDraft[];
}

export const emptyVariation = (): VariationDraft => ({
  characteristics: [],
  options: [{ values: {}, price: 0, isMarkedAvailable: true }],
});

const key = (value: string) => value.trim().toLowerCase();

/**
 * Editor de variación de un producto (FR-008 a FR-011). Sin características hay exactamente una
 * opción; con características se agrega una fila por opción y se indica un valor para cada una.
 */
@Component({
  selector: 'app-variation-editor',
  template: `
    <fieldset class="border rounded p-3 mb-3">
      <legend class="float-none w-auto px-2 fs-6">Variantes</legend>
      <div class="d-flex flex-wrap gap-2 align-items-center mb-3">
        @for (name of draft().characteristics; track name) {
          <span class="badge bg-info d-flex align-items-center">
            {{ name }}
            <button type="button" class="btn-close btn-close-white ms-2" [attr.aria-label]="'Quitar ' + name"
              (click)="removeCharacteristic(name)"></button>
          </span>
        }
        <input #newName class="form-control form-control-sm w-auto" placeholder="Nueva característica (ej. tamaño)"
          (keydown.enter)="$event.preventDefault(); addCharacteristic(newName.value); newName.value = ''" />
        <button type="button" class="btn btn-sm btn-outline-primary" (click)="addCharacteristic(newName.value); newName.value = ''">
          <i class="fa-solid fa-plus me-1"></i>Agregar característica
        </button>
      </div>
      @if (error) {
        <div class="alert alert-warning py-1">{{ error }}</div>
      }
      <table class="table table-sm align-middle">
        <thead>
          <tr>
            @for (name of draft().characteristics; track name) {
              <th>{{ name }}</th>
            }
            <th>Precio</th>
            <th>Disponible</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          @for (option of draft().options; track $index) {
            <tr>
              @for (name of draft().characteristics; track name) {
                <td>
                  <input class="form-control form-control-sm" [value]="option.values[name] ?? ''"
                    (input)="setValue($index, name, $any($event.target).value)" />
                </td>
              }
              <td>
                <input type="number" min="0" step="0.01" class="form-control form-control-sm" [value]="option.price"
                  (input)="setPrice($index, $any($event.target).valueAsNumber)" />
              </td>
              <td>
                <input type="checkbox" class="form-check-input" [checked]="option.isMarkedAvailable"
                  (change)="setAvailable($index, $any($event.target).checked)" />
              </td>
              <td class="text-end">
                @if (draft().options.length > 1) {
                  <button type="button" class="btn btn-sm btn-outline-danger" aria-label="Quitar opción" (click)="removeOption($index)">
                    <i class="fa-solid fa-trash"></i>
                  </button>
                }
              </td>
            </tr>
          }
        </tbody>
      </table>
      @if (hasCharacteristics()) {
        <button type="button" class="btn btn-sm btn-outline-primary" (click)="addOption()">
          <i class="fa-solid fa-plus me-1"></i>Agregar opción
        </button>
      }
    </fieldset>
  `,
})
export class VariationEditor {
  readonly draft = model.required<VariationDraft>();
  protected readonly hasCharacteristics = computed(() => this.draft().characteristics.length > 0);
  protected error = '';

  protected addCharacteristic(raw: string): void {
    const name = raw.trim();
    const current = this.draft();
    if (!name) {
      return;
    }
    if (current.characteristics.some((c) => key(c) === key(name))) {
      this.error = `La característica '${name}' ya existe.`;
      return;
    }
    this.error = '';
    this.draft.set({
      characteristics: [...current.characteristics, name],
      options: current.options.map((o) => ({ ...o, values: { ...o.values, [name]: '' } })),
    });
  }

  protected removeCharacteristic(name: string): void {
    const current = this.draft();
    const characteristics = current.characteristics.filter((c) => c !== name);
    const options = current.options.map((o) => {
      const { [name]: _removed, ...values } = o.values;
      return { ...o, values };
    });
    // Sin características un producto tiene exactamente una opción (invariante 1).
    this.draft.set({ characteristics, options: characteristics.length === 0 ? options.slice(0, 1) : options });
  }

  protected addOption(): void {
    const current = this.draft();
    const values = Object.fromEntries(current.characteristics.map((c) => [c, '']));
    this.draft.set({ ...current, options: [...current.options, { values, price: 0, isMarkedAvailable: true }] });
  }

  protected removeOption(index: number): void {
    const current = this.draft();
    this.draft.set({ ...current, options: current.options.filter((_, i) => i !== index) });
  }

  protected setValue(index: number, name: string, value: string): void {
    this.patch(index, (o) => ({ ...o, values: { ...o.values, [name]: value } }));
  }

  protected setPrice(index: number, price: number): void {
    this.patch(index, (o) => ({ ...o, price }));
  }

  protected setAvailable(index: number, isMarkedAvailable: boolean): void {
    this.patch(index, (o) => ({ ...o, isMarkedAvailable }));
  }

  private patch(index: number, change: (option: OptionDraft) => OptionDraft): void {
    const current = this.draft();
    this.draft.set({ ...current, options: current.options.map((o, i) => (i === index ? change(o) : o)) });
  }
}
