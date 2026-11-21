import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Login } from './login';
import { AuthService } from '../../../core/auth/auth.service';

describe('Login', () => {
  let authService: { login: ReturnType<typeof vi.fn> };
  let router: { navigate: ReturnType<typeof vi.fn> };

  function create() {
    TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router },
      ],
    });
    const fixture = TestBed.createComponent(Login);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  beforeEach(() => {
    authService = { login: vi.fn() };
    router = { navigate: vi.fn() };
  });

  it('should create', () => {
    expect(create()).toBeTruthy();
  });

  it('não chama o login com formulário inválido e marca os campos', () => {
    const c = create();
    c.form.setValue({ email: 'invalido', password: '123' });

    c.submit();

    expect(authService.login).not.toHaveBeenCalled();
    expect(c.form.controls.email.touched).toBe(true);
  });

  it('faz login e navega para /events', () => {
    authService.login.mockReturnValue(of({ token: 't' }));
    const c = create();
    c.form.setValue({ email: 'a@a.com', password: '123456' });

    c.submit();

    expect(authService.login).toHaveBeenCalledWith('a@a.com', '123456');
    expect(router.navigate).toHaveBeenCalledWith(['/events']);
  });

  it('mostra mensagem de erro quando o login falha', () => {
    authService.login.mockReturnValue(throwError(() => new Error('401')));
    const c = create();
    c.form.setValue({ email: 'a@a.com', password: '123456' });

    c.submit();

    expect(c.errorMessage()).toBe('E-mail ou senha inválidos.');
    expect(c.loading()).toBe(false);
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
