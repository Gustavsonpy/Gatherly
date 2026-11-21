import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  let http: HttpClient;
  let controller: HttpTestingController;
  let token: string | null;
  const logout = vi.fn();
  const navigate = vi.fn();

  beforeEach(() => {
    token = 'meu-token';
    logout.mockReset();
    navigate.mockReset();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: { getToken: () => token, logout } },
        { provide: Router, useValue: { navigate } },
      ],
    });

    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('adiciona o header Authorization em requisições para a API', () => {
    http.get('http://localhost:5008/api/event').subscribe();

    const req = controller.expectOne('http://localhost:5008/api/event');
    expect(req.request.headers.get('Authorization')).toBe('Bearer meu-token');
    req.flush([]);
  });

  it('não adiciona o header para URLs externas', () => {
    http.get('https://photon.komoot.io/api/').subscribe();

    const req = controller.expectOne('https://photon.komoot.io/api/');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('não adiciona o header quando não há token', () => {
    token = null;
    http.get('http://localhost:5008/api/event').subscribe();

    const req = controller.expectOne('http://localhost:5008/api/event');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush([]);
  });

  it('em 401 faz logout, redireciona para /login e repassa o erro', () => {
    const onError = vi.fn();
    http.get('http://localhost:5008/api/user/me').subscribe({ error: onError });

    controller
      .expectOne('http://localhost:5008/api/user/me')
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(logout).toHaveBeenCalled();
    expect(navigate).toHaveBeenCalledWith(['/login']);
    expect(onError).toHaveBeenCalled();
  });

  it('em outros erros apenas repassa o erro', () => {
    const onError = vi.fn();
    http.get('http://localhost:5008/api/event').subscribe({ error: onError });

    controller
      .expectOne('http://localhost:5008/api/event')
      .flush(null, { status: 500, statusText: 'Server Error' });

    expect(logout).not.toHaveBeenCalled();
    expect(onError).toHaveBeenCalled();
  });
});
