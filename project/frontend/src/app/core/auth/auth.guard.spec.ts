import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';

describe('authGuard', () => {
  const navigate = vi.fn();

  function run(authenticated: boolean) {
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { isAuthenticated: () => authenticated } },
        { provide: Router, useValue: { navigate } },
      ],
    });

    return TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    );
  }

  beforeEach(() => navigate.mockReset());

  it('permite acesso quando autenticado', () => {
    expect(run(true)).toBe(true);
    expect(navigate).not.toHaveBeenCalled();
  });

  it('bloqueia e redireciona para /login quando não autenticado', () => {
    expect(run(false)).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });
});
