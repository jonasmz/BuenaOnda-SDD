import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Characteristic, SellableOption } from '../../models/product';
import { OptionForm } from './option-form';

const option = (overrides: Partial<SellableOption> = {}): SellableOption => ({
  id: 'o1',
  values: { tamaño: 'chica' },
  price: 1000,
  description: null,
  imageUrl: null,
  isMarkedAvailable: true,
  isActive: true,
  isAvailable: true,
  isVisibleToPublic: true,
  ...overrides,
});

@Component({
  imports: [OptionForm],
  template: `<app-option-form productId="p1" [option]="option()" [characteristics]="characteristics"
    (productChanged)="changed = $event" (removed)="removed = true" />`,
})
class Host {
  readonly option = signal(option());
  readonly characteristics: Characteristic[] = [{ id: 'c1', name: 'tamaño' }];
  changed: unknown;
  removed = false;
}

describe('OptionForm', () => {
  const setup = async (opt = option()) => {
    await TestBed.configureTestingModule({
      imports: [Host],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    fixture.componentInstance.option.set(opt);
    await fixture.whenStable();
    return { fixture, http: TestBed.inject(HttpTestingController), el: fixture.nativeElement as HTMLElement };
  };

  it('shows the option values, price and the availability switch on', async () => {
    const { el } = await setup();
    expect(el.textContent).toContain('chica');
    expect((el.querySelector('input[type=number]') as HTMLInputElement).value).toBe('1000');
    expect((el.querySelector('input[role=switch]') as HTMLInputElement).checked).toBe(true);
  });

  it('toggling availability calls the availability endpoint', async () => {
    const { el, http } = await setup();
    const toggle = el.querySelector('input[role=switch]') as HTMLInputElement;
    toggle.checked = false;
    toggle.dispatchEvent(new Event('change'));
    const req = http.expectOne('/api/admin/catalog/products/p1/options/o1/availability');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ isMarkedAvailable: false });
  });

  it('a 409 on delete shows the message and offers deactivation', async () => {
    const { fixture, el, http } = await setup();
    (Array.from(el.querySelectorAll('button')).find((b) => b.textContent?.includes('Eliminar')) as HTMLButtonElement).click();
    http
      .expectOne('/api/admin/catalog/products/p1/options/o1')
      .flush({ detail: 'La opción fue referenciada.' }, { status: 409, statusText: 'Conflict' });
    await fixture.whenStable();
    fixture.detectChanges();
    expect(el.querySelector('[role=alert]')?.textContent).toContain('referenciada');
    expect(Array.from(el.querySelectorAll('[role=alert] button')).some((b) => b.textContent?.includes('Dar de baja'))).toBe(true);
  });

  it('an inactive option offers reactivation', async () => {
    const { el } = await setup(option({ isActive: false, isAvailable: false }));
    expect(el.textContent).toContain('Reactivar');
  });
});
