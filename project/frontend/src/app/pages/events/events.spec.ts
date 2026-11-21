import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Events } from './events';
import { EventService } from '../../core/event/event.service';
import { AuthService } from '../../core/auth/auth.service';
import { makeEvent, makeJwt, stubLocalStorage } from '../../testing/test-helpers';
import { provideHttpClient } from '@angular/common/http';

describe('Events', () => {
  let eventService: { getFutureEvents: ReturnType<typeof vi.fn>; getEventsByCity: ReturnType<typeof vi.fn> };

  function create() {
    TestBed.configureTestingModule({
      imports: [Events],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        { provide: EventService, useValue: eventService },
      ],
    });
    const fixture = TestBed.createComponent(Events);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  beforeEach(() => {
    stubLocalStorage({ gatherly_token: makeJwt({ name: 'Ana Maria Silva' }) });
    vi.spyOn(console, 'log').mockImplementation(() => {});
    eventService = {
      getFutureEvents: vi.fn(() => of([makeEvent({ id: 'a' })])),
      getEventsByCity: vi.fn(() => of([makeEvent({ id: 'b' })])),
    };
  });

  afterEach(() => vi.unstubAllGlobals());

  it('carrega eventos futuros e da cidade', () => {
    const c = create();
    expect(c.events().map((e) => e.id)).toEqual(['a']);
    expect(c.cityEvents().map((e) => e.id)).toEqual(['b']);
    expect(c.loading()).toBe(false);
  });

  it('encerra o loading mesmo quando as requisições falham', () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    eventService.getFutureEvents.mockReturnValue(throwError(() => new Error('x')));
    eventService.getEventsByCity.mockReturnValue(throwError(() => new Error('x')));

    const c = create();

    expect(c.events()).toEqual([]);
    expect(c.loading()).toBe(false);
  });

  it('logout limpa a sessão e volta para o login', () => {
    const c = create();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    c.logout();

    expect(TestBed.inject(AuthService).isAuthenticated()).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });
});
