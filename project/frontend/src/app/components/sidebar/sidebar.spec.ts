import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { Sidebar } from './sidebar';
import { UserDTO, UserService } from '../../core/user/user.service';
import { makeJwt, stubLocalStorage } from '../../testing/test-helpers';

const me: UserDTO = {
  id: 'user-1',
  name: 'Bruno Costa Lima',
  email: 'bruno@email.com',
  birthday: '2000-01-01',
  description: null,
  city: null,
  urlPhoto: 'http://localhost/uploads/users/foto.jpg',
};

/** Cria o Sidebar com um UserService falso, sem chamar a API de verdade. */
async function create(getMe: () => Observable<UserDTO> = () => of(me)): Promise<ComponentFixture<Sidebar>> {
  await TestBed.configureTestingModule({
    imports: [Sidebar],
    providers: [provideRouter([]), { provide: UserService, useValue: { getMe: vi.fn(getMe) } }],
  }).compileComponents();

  const fixture = TestBed.createComponent(Sidebar);
  await fixture.whenStable();
  return fixture;
}

describe('Sidebar', () => {
  beforeEach(() => {
    stubLocalStorage({ gatherly_token: makeJwt({ name: 'Ana Maria da Silva' }) });
    vi.spyOn(console, 'log').mockImplementation(() => {});
  });

  afterEach(() => vi.unstubAllGlobals());

  it('should create', async () => {
    const fixture = await create();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('mostra o primeiro e o último nome do usuário logado', async () => {
    const fixture = await create();
    expect(fixture.componentInstance.username()).toBe('Ana Silva');
  });

  it('mostra o nome e a foto que vêm da API', async () => {
    const fixture = await create();

    expect(fixture.componentInstance.usernamee()).toBe('Bruno Lima');
    expect(fixture.componentInstance.userPhoto()).toBe('http://localhost/uploads/users/foto.jpg');
  });

  it('quando a API falha, usa o nome do token', async () => {
    const fixture = await create(() => throwError(() => new Error('401')));

    expect(fixture.componentInstance.usernamee()).toBe('Ana Silva');
  });

  it('transformUsername lida com nome único', async () => {
    const fixture = await create();
    expect(fixture.componentInstance.transformUsername('Ana')).toBe('Ana Ana');
  });

  it('não mostra o link de administração para usuário comum', async () => {
    const fixture = await create();
    fixture.detectChanges();

    expect(fixture.componentInstance.isAdmin()).toBe(false);
    expect(fixture.nativeElement.querySelector('a[href="/admin"]')).toBeNull();
  });
});

describe('Sidebar (admin)', () => {
  beforeEach(() => {
    stubLocalStorage({ gatherly_token: makeJwt({ name: 'Admin Chefe', role: 'Admin' }) });
    vi.spyOn(console, 'log').mockImplementation(() => {});
  });

  afterEach(() => vi.unstubAllGlobals());

  it('mostra o link de administração para admin', async () => {
    const fixture = await create();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('a[href="/admin"]')?.textContent).toContain('Administração');
  });
});
