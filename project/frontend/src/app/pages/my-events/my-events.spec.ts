import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { MyEvents } from './my-events';
import { EventService } from '../../core/event/event.service';
import { makeEvent, makeJwt, stubLocalStorage } from '../../testing/test-helpers';

describe('MyEvents', () => {
  let eventService: {
    getMyEvents: ReturnType<typeof vi.fn>;
    getParticipants: ReturnType<typeof vi.fn>;
    cancelEvent: ReturnType<typeof vi.fn>;
  };

  const events = [
    makeEvent({ id: 'a', status: 'upcoming', userId: 'USER-1' }),
    makeEvent({ id: 'b', status: 'ongoing', userId: 'other' }),
    makeEvent({ id: 'c', status: 'canceled' }),
  ];

  function create() {
    TestBed.configureTestingModule({
      imports: [MyEvents],
      providers: [provideHttpClient(), provideRouter([]), { provide: EventService, useValue: eventService }],
    });
    const fixture = TestBed.createComponent(MyEvents);
    fixture.detectChanges();
    return fixture;
  }

  beforeEach(() => {
    stubLocalStorage({ gatherly_token: makeJwt({ id: 'user-1' }) });
    vi.spyOn(console, 'log').mockImplementation(() => {});
    vi.spyOn(console, 'error').mockImplementation(() => {});
    eventService = {
      getMyEvents: vi.fn(() => of(events)),
      getParticipants: vi.fn(() => of([])),
      cancelEvent: vi.fn(),
    };
  });

  afterEach(() => vi.unstubAllGlobals());

  it('filtra os eventos pela aba ativa', () => {
    const fixture = create();
    const c = fixture.componentInstance;

    expect(c.visibleEvents().map((e) => e.id)).toEqual(['a']);

    c.setActiveTab('ongoing');
    fixture.detectChanges();
    expect(c.visibleEvents().map((e) => e.id)).toEqual(['b']);

    c.setActiveTab('canceled');
    expect(c.visibleEvents().map((e) => e.id)).toEqual(['c']);
  });

  it('sinaliza falha quando não consegue carregar', () => {
    eventService.getMyEvents.mockReturnValue(throwError(() => new Error('x')));
    const c = create().componentInstance;
    expect(c.failed()).toBe(true);
    expect(c.loading()).toBe(false);
  });

  it('identifica o organizador ignorando maiúsculas', () => {
    const c = create().componentInstance;
    expect(c.isOrganizer(events[0])).toBe(true);
    expect(c.isOrganizer(events[1])).toBe(false);
  });

  it('não cancela se o usuário não confirmar', () => {
    vi.stubGlobal('confirm', () => false);
    const c = create().componentInstance;

    c.cancelEvent(events[0]);

    expect(eventService.cancelEvent).not.toHaveBeenCalled();
  });

  it('cancela o evento e atualiza o status localmente', () => {
    vi.stubGlobal('confirm', () => true);
    eventService.cancelEvent.mockReturnValue(of(undefined));
    const c = create().componentInstance;

    c.cancelEvent(events[0]);

    expect(eventService.cancelEvent).toHaveBeenCalledWith('a');
    expect(c.events().find((e) => e.id === 'a')?.status).toBe('canceled');
  });

  it('mostra o erro quando o cancelamento falha', () => {
    vi.stubGlobal('confirm', () => true);
    eventService.cancelEvent.mockReturnValue(
      throwError(() => ({ error: { errors: ['Só é possível cancelar eventos futuros.'] } })),
    );
    const c = create().componentInstance;

    c.cancelEvent(events[0]);

    expect(c.actionError()).toBe('Só é possível cancelar eventos futuros.');
  });

  it('usa mensagem padrão quando o erro de cancelamento não tem detalhes', () => {
    vi.stubGlobal('confirm', () => true);
    eventService.cancelEvent.mockReturnValue(throwError(() => ({})));
    const c = create().componentInstance;

    c.cancelEvent(events[0]);

    expect(c.actionError()).toBe('Não foi possível cancelar o evento.');
  });
});
