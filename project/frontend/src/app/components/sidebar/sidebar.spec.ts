import { provideHttpClient } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Sidebar } from './sidebar';
import { makeJwt, stubLocalStorage } from '../../testing/test-helpers';

describe('Sidebar', () => {
  let component: Sidebar;
  let fixture: ComponentFixture<Sidebar>;

  beforeEach(async () => {
    stubLocalStorage({ gatherly_token: makeJwt({ name: 'Ana Maria da Silva' }) });
    vi.spyOn(console, 'log').mockImplementation(() => {});

    await TestBed.configureTestingModule({
      imports: [Sidebar],
      providers: [provideHttpClient(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(Sidebar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  afterEach(() => vi.unstubAllGlobals());

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('mostra o primeiro e o último nome do usuário logado', () => {
    expect(component.username()).toBe('Ana Silva');
  });

  it('transformUsername lida com nome único', () => {
    expect(component.transformUsername('Ana')).toBe('Ana Ana');
  });

  it('não mostra o link de administração para usuário comum', () => {
    fixture.detectChanges();
    expect(component.isAdmin()).toBe(false);
    expect(fixture.nativeElement.querySelector('a[href="/admin"]')).toBeNull();
  });
});

describe('Sidebar (admin)', () => {
  beforeEach(() => {
    stubLocalStorage({ gatherly_token: makeJwt({ name: 'Admin Chefe', role: 'Admin' }) });
    vi.spyOn(console, 'log').mockImplementation(() => {});
    TestBed.configureTestingModule({
      imports: [Sidebar],
      providers: [provideHttpClient(), provideRouter([])],
    });
  });

  afterEach(() => vi.unstubAllGlobals());

  it('mostra o link de administração para admin', async () => {
    const fixture = TestBed.createComponent(Sidebar);
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('a[href="/admin"]')?.textContent).toContain('Administração');
  });
});
