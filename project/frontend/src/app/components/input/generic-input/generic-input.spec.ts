import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl } from '@angular/forms';
import { GenericInput } from './generic-input';

describe('GenericInput', () => {
  let component: GenericInput;
  let fixture: ComponentFixture<GenericInput>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GenericInput],
    }).compileComponents();

    fixture = TestBed.createComponent(GenericInput);
    fixture.componentRef.setInput('id', 'email');
    fixture.componentRef.setInput('label', 'E-mail');
    fixture.componentRef.setInput('control', new FormControl(''));
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('alterna a visibilidade da senha', () => {
    fixture.componentRef.setInput('type', 'password');
    fixture.detectChanges();
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.type).toBe('password');

    (fixture.nativeElement.querySelector('button') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(component.passwordVisible()).toBe(true);
    expect(input.type).toBe('text');
  });

  it('exibe o erro do backend quando o controle é inválido e tocado', () => {
    const control = new FormControl('');
    fixture.componentRef.setInput('control', control);
    fixture.detectChanges();

    control.setErrors({ backend: 'E-mail já existe.' });
    control.markAsTouched();
    fixture.detectChanges();

    const p: HTMLElement = fixture.nativeElement.querySelector('p');
    expect(p.textContent?.trim()).toBe('E-mail já existe.');
    expect(p.classList.contains('invisible')).toBe(false);
  });
});
