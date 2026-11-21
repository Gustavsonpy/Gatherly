import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Register } from './register';
import { UserService } from '../../core/user/user.service';

describe('Register', () => {
  let userService: { create: ReturnType<typeof vi.fn> };
  let router: { navigate: ReturnType<typeof vi.fn> };

  const validForm = {
    name: 'Ana',
    email: 'ana@email.com',
    password: '123456',
    confirm_password: '123456',
    birthDate: '2000-01-01',
  };

  function create() {
    TestBed.configureTestingModule({
      imports: [Register],
      providers: [
        { provide: UserService, useValue: userService },
        { provide: Router, useValue: router },
      ],
    });
    const fixture = TestBed.createComponent(Register);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  beforeEach(() => {
    userService = { create: vi.fn() };
    router = { navigate: vi.fn() };
  });

  it('should create', () => {
    expect(create()).toBeTruthy();
  });

  it('não envia quando as senhas não coincidem', () => {
    const c = create();
    c.form.setValue({ ...validForm, confirm_password: '654321' });

    c.submit();

    expect(c.form.controls.confirm_password.hasError('mismatch')).toBe(true);
    expect(userService.create).not.toHaveBeenCalled();
  });

  it('cria o usuário e navega para /login', () => {
    userService.create.mockReturnValue(of({}));
    const c = create();
    c.form.setValue(validForm);

    c.submit();

    expect(userService.create).toHaveBeenCalledWith({
      name: 'Ana', email: 'ana@email.com', password: '123456', birthday: '2000-01-01',
    });
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('aplica erros de campo retornados pelo backend', () => {
    userService.create.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 400,
            error: { fieldErrors: { email: ['Já existe uma conta com esse e-mail.'] } },
          }),
      ),
    );
    const c = create();
    c.form.setValue(validForm);

    c.submit();

    expect(c.form.controls.email.errors).toEqual({ backend: 'Já existe uma conta com esse e-mail.' });
    expect(c.errorMessage()).toBeNull();
    expect(c.loading()).toBe(false);
  });
});
