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
});