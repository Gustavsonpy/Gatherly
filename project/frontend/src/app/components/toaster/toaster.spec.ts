import { TestBed } from '@angular/core/testing';
import { ToasterComponent } from './toaster';
import { ToastService } from '../../core/toaster/toaster.service';

describe('ToasterComponent', () => {
  it('renderiza os toasts e remove ao clicar', () => {
    const fixture = TestBed.createComponent(ToasterComponent);
    const service = TestBed.inject(ToastService);

    service.error('Falhou');
    service.success('Deu certo');
    fixture.detectChanges();

    const items: HTMLElement[] = Array.from(fixture.nativeElement.querySelectorAll('[role]'));
    expect(items.map((i) => i.getAttribute('role'))).toEqual(['alert', 'status']);
    expect(items[0].textContent).toContain('Falhou');

    items[0].click();
    fixture.detectChanges();

    expect(service.toasts().map((t) => t.message)).toEqual(['Deu certo']);
  });
});
