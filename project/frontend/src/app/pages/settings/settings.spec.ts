import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { Settings } from './settings';
import { UserService } from '../../core/user/user.service';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/toaster/toaster.service';

describe('Settings', () => {
  const navigate = vi.fn(() => Promise.resolve(true));
  const logout = vi.fn();

  function create(): Settings {
    TestBed.configureTestingModule({
      imports: [Settings],
      providers: [
        { provide: Router, useValue: { navigate } },
        { provide: AuthService, useValue: { logout } },
        { provide: UserService, useValue: {} },
        { provide: ToastService, useValue: {} },
      ],
    });
    // Template vazio: o teste foca na lógica do componente, não na tela
    TestBed.overrideComponent(Settings, { set: { template: '', imports: [] } });
    return TestBed.createComponent(Settings).componentInstance;
  }

  beforeEach(() => {
    navigate.mockClear();
    logout.mockClear();
  });

  it('logout encerra a sessão e volta para o login', () => {
    create().logout();

    expect(logout).toHaveBeenCalled();
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });
});
