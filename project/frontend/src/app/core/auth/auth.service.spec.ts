import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';
import { makeJwt, stubLocalStorage } from '../../testing/test-helpers';

describe('AuthService', () => {
  let store: Map<string, string>;

  function setup(initialToken?: string) {
    store = stubLocalStorage(initialToken ? { gatherly_token: initialToken } : {});
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    return {
      service: TestBed.inject(AuthService),
      http: TestBed.inject(HttpTestingController),
    };
  }

  afterEach(() => vi.unstubAllGlobals());

  it('começa não autenticado quando não há token salvo', () => {
    const { service } = setup();

    expect(service.isAuthenticated()).toBe(false);
    expect(service.getToken()).toBeNull();
    expect(service.currentUser()).toBeNull();
  });

  it('restaura o token salvo no localStorage e decodifica o usuário', () => {
    const token = makeJwt({ id: 'abc', email: 'a@b.com', name: 'Ana Silva' });
    const { service } = setup(token);

    expect(service.isAuthenticated()).toBe(true);
    expect(service.currentUser()).toEqual({ id: 'abc', email: 'a@b.com', name: 'Ana Silva', isAdmin: false });
    expect(service.isAdmin()).toBe(false);
  });

  it('identifica admin pela claim de role (string ou array)', () => {
    expect(setup(makeJwt({ role: 'Admin' })).service.isAdmin()).toBe(true);
    TestBed.resetTestingModule();
    expect(setup(makeJwt({ role: ['Outro', 'Admin'] })).service.isAdmin()).toBe(true);
    TestBed.resetTestingModule();
    expect(setup(makeJwt({ role: 'Outro' })).service.isAdmin()).toBe(false);
  });

  it('isAdmin é falso sem token', () => {
    expect(setup().service.isAdmin()).toBe(false);
  });

  it('retorna currentUser nulo quando o token é inválido', () => {
    const { service } = setup('token-invalido');

    expect(service.isAuthenticated()).toBe(true);
    expect(service.currentUser()).toBeNull();
  });

  it('login faz POST e salva o token recebido', () => {
    const { service, http } = setup();
    const token = makeJwt();

    service.login('user@email.com', '123456').subscribe();

    const req = http.expectOne('http://localhost:5008/api/login');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'user@email.com', password: '123456' });
    req.flush({ token });

    expect(service.getToken()).toBe(token);
    expect(store.get('gatherly_token')).toBe(token);
    http.verify();
  });

  it('logout limpa o token da memória e do localStorage', () => {
    const { service } = setup(makeJwt());

    service.logout();

    expect(service.isAuthenticated()).toBe(false);
    expect(store.has('gatherly_token')).toBe(false);
  });
});
