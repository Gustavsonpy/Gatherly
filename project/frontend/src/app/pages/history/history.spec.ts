import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { HistoryPage } from './history';
import { EventService } from '../../core/event/event.service';
import { makeEvent, makeJwt, stubLocalStorage } from '../../testing/test-helpers';

describe('HistoryPage', () => {
  let eventService: { getMyEvents: ReturnType<typeof vi.fn>; getParticipants: ReturnType<typeof vi.fn> };

  const events = [
    makeEvent({ id: 'jan1', status: 'done', dateTimeStart: '2025-01-05T12:00:00Z', userId: 'user-1' }),
    makeEvent({ id: 'jan2', status: 'done', dateTimeStart: '2025-01-20T12:00:00Z', userId: 'other' }),
    makeEvent({ id: 'mar', status: 'done', dateTimeStart: '2025-03-10T12:00:00Z', userId: 'other' }),
    makeEvent({ id: 'left', status: 'done', isParticipating: false, userId: 'other' }),
    makeEvent({ id: 'future', status: 'upcoming' }),
  ];

  function create() {
    TestBed.configureTestingModule({
      imports: [HistoryPage],
      providers: [provideHttpClient(), provideRouter([]), { provide: EventService, useValue: eventService }],
    });
    const fixture = TestBed.createComponent(HistoryPage);
    fixture.detectChanges();
    return fixture;
  }

  beforeEach(() => {
    stubLocalStorage({ gatherly_token: makeJwt({ id: 'user-1' }) });
    vi.spyOn(console, 'log').mockImplementation(() => {});
    vi.spyOn(console, 'error').mockImplementation(() => {});
    eventService = { getMyEvents: vi.fn(() => of(events)), getParticipants: vi.fn(() => of([])) };
  });

  afterEach(() => vi.unstubAllGlobals());

  it('mostra apenas eventos concluídos em que o usuário participou, agrupados por mês', () => {
    const c = create().componentInstance;

    expect(c.visibleEvents().map((e) => e.id).sort()).toEqual(['jan1', 'jan2', 'mar']);
    expect(c.groups().map((g) => g.events.map((e) => e.id))).toEqual([['mar'], ['jan2', 'jan1']]);
  });

  it('na aba "organizou" mostra apenas eventos do usuário', () => {
    const fixture = create();
    const c = fixture.componentInstance;

    c.setActiveTab('organized');
    fixture.detectChanges();

    expect(c.visibleEvents().map((e) => e.id)).toEqual(['jan1']);
    expect(c.activeTabInfo().label).toBe('Organizou');
  });

  it('sinaliza falha quando não consegue carregar', () => {
    eventService.getMyEvents.mockReturnValue(throwError(() => new Error('x')));
    const c = create().componentInstance;
    expect(c.failed()).toBe(true);
    expect(c.loading()).toBe(false);
  });
});
