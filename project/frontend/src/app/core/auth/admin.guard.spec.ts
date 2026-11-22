import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { adminGuard } from './admin.guard';
import { AuthService } from './auth.service';

describe('adminGuard', () => {
  const navigate = vi.fn();

  function run(isAdmin: boolean, isAuthenticated = true) {
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { isAdmin: () => isAdmin, isAuthenticated: () => isAuthenticated } },
        { provide: Router, useValue: { navigate } },
      ],
    });

    return TestBed.runInInjectionContext(() =>
      adminGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    );
  }

  beforeEach(() => navigate.mockReset());

  it('permite acesso para admin', () => {
    expect(run(true)).toBe(true);
    expect(navigate).not.toHaveBeenCalled();
  });

  it('manda usuário comum para a home', () => {
    expect(run(false)).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/homepage']);
  });

  it('manda quem não está logado para o login', () => {
    expect(run(false, false)).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });
});
