import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { emptyVariation, VariationDraft, VariationEditor } from './variation-editor';

@Component({
  imports: [VariationEditor],
  template: `<app-variation-editor [(draft)]="draft" />`,
})
class Host {
  readonly draft = signal<VariationDraft>(emptyVariation());
}

describe('VariationEditor', () => {
  const setup = async () => {
    await TestBed.configureTestingModule({ imports: [Host] }).compileComponents();
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const editor = fixture.debugElement.children[0].componentInstance as {
      addCharacteristic(n: string): void;
      removeCharacteristic(n: string): void;
      addOption(): void;
      removeOption(i: number): void;
      setValue(i: number, n: string, v: string): void;
    };
    return { fixture, editor, draft: () => fixture.componentInstance.draft() };
  };

  it('starts with one option and no characteristics', async () => {
    const { draft } = await setup();
    expect(draft().characteristics).toEqual([]);
    expect(draft().options.length).toBe(1);
  });

  it('adds a characteristic to every option and rejects duplicates ignoring case', async () => {
    const { editor, draft } = await setup();
    editor.addCharacteristic('tamaño');
    editor.addCharacteristic(' TAMAÑO ');
    expect(draft().characteristics).toEqual(['tamaño']);
    expect(draft().options[0].values).toEqual({ tamaño: '' });
  });

  it('adds options with an empty value per characteristic and keeps values typed by the user', async () => {
    const { editor, draft } = await setup();
    editor.addCharacteristic('tamaño');
    editor.setValue(0, 'tamaño', 'chica');
    editor.addOption();
    editor.setValue(1, 'tamaño', 'grande');
    expect(draft().options.map((o) => o.values['tamaño'])).toEqual(['chica', 'grande']);
  });

  it('removing the last characteristic leaves exactly one option', async () => {
    const { editor, draft } = await setup();
    editor.addCharacteristic('tamaño');
    editor.addOption();
    editor.removeCharacteristic('tamaño');
    expect(draft().characteristics).toEqual([]);
    expect(draft().options.length).toBe(1);
    expect(draft().options[0].values).toEqual({});
  });
});
