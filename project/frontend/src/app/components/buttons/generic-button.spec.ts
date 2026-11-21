import { TestBed } from '@angular/core/testing';
import { GenericButton } from './generic-button';

describe('GenericButton', () => {
  function create(inputs: Record<string, unknown> = {}) {
    const fixture = TestBed.createComponent(GenericButton);
    for (const [k, v] of Object.entries(inputs)) fixture.componentRef.setInput(k, v);
    fixture.detectChanges();
    return fixture;
  }

  it('aplica as classes da variante escolhida', () => {
    expect(create().componentInstance.classes()).toContain('bg-primary');
    expect(create({ variant: 'secondary' }).componentInstance.classes()).toContain('bg-white');
    expect(create({ variant: 'red_white' }).componentInstance.classes()).toContain('text-red-500');
  });

  it('emite clicked ao clicar', () => {
    const fixture = create({ type: 'submit' });
    const clicked = vi.fn();
    fixture.componentInstance.clicked.subscribe(clicked);

    const button: HTMLButtonElement = fixture.nativeElement.querySelector('button');
    button.click();

    expect(button.type).toBe('submit');
    expect(clicked).toHaveBeenCalled();
  });

  it('desabilita o botão', () => {
    const button: HTMLButtonElement = create({ disabled: true }).nativeElement.querySelector('button');
    expect(button.disabled).toBe(true);
  });
});
