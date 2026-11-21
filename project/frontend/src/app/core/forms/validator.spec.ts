import { FormControl, FormGroup, Validators } from '@angular/forms';
import { passwordsMatchValidator } from './validator';

describe('passwordsMatchValidator', () => {
  const makeForm = () =>
    new FormGroup(
      {
        password: new FormControl('123456'),
        confirm_password: new FormControl('123456', Validators.minLength(6)),
      },
      { validators: passwordsMatchValidator },
    );

  it('não marca erro quando as senhas coincidem', () => {
    const form = makeForm();
    expect(form.controls.confirm_password.errors).toBeNull();
    expect(form.valid).toBe(true);
  });

  it('marca mismatch quando as senhas são diferentes', () => {
    const form = makeForm();
    form.controls.confirm_password.setValue('654321');
    expect(form.controls.confirm_password.hasError('mismatch')).toBe(true);
  });

  it('remove o mismatch quando as senhas voltam a coincidir', () => {
    const form = makeForm();
    form.controls.confirm_password.setValue('654321');
    form.controls.password.setValue('654321');
    expect(form.controls.confirm_password.errors).toBeNull();
  });

  it('mantém outros erros ao remover o mismatch', () => {
    const control = new FormControl('abc');
    control.setErrors({ mismatch: true, minlength: true });
    const group = new FormGroup({ password: new FormControl('abc'), confirm_password: control });

    passwordsMatchValidator(group);

    expect(control.errors).toEqual({ minlength: true });
  });

  it('retorna null quando não existe confirm_password', () => {
    expect(passwordsMatchValidator(new FormGroup({ password: new FormControl('x') }))).toBeNull();
  });
});
