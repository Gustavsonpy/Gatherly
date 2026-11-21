import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { EventRow } from './event-row';
import { EventService } from '../../core/event/event.service';
import { makeEvent } from '../../testing/test-helpers';

describe('EventRow', () => {
  const participant = (id: string) => ({ id, name: id, urlPhoto: null, userScore: 0, ratingsCount: 0 });
  let eventService: { getParticipants: ReturnType<typeof vi.fn> };

  function create(inputs: Record<string, unknown> = {}) {
    TestBed.configureTestingModule({
      imports: [EventRow],
      providers: [provideRouter([]), { provide: EventService, useValue: eventService }],
    });
    const fixture = TestBed.createComponent(EventRow);
    fixture.componentRef.setInput('event', makeEvent());
    for (const [k, v] of Object.entries(inputs)) fixture.componentRef.setInput(k, v);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  beforeEach(() => {
    eventService = { getParticipants: vi.fn(() => of(['a', 'b', 'c', 'd', 'e', 'f'].map(participant))) };
  });

  it('carrega participantes e limita os avatares visíveis', () => {
    const c = create();
    expect(c.visibleParticipants().length).toBe(4);
    expect(c.extraParticipants()).toBe(2);
  });

  it('trata erro ao carregar participantes', () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    eventService.getParticipants.mockReturnValue(throwError(() => new Error('x')));
    expect(create().participants()).toEqual([]);
  });

  it('indica quando o evento está lotado', () => {
    expect(create({ event: makeEvent({ participantsCount: 10, maxCapacity: 10 }) }).isFull()).toBe(true);
  });

  it('permite cancelar apenas se for organizador e o evento não começou', () => {
    expect(create({ isOrganizer: true }).canCancel()).toBe(true);
  });

  it('não permite cancelar evento em andamento ou quando não é organizador', () => {
    expect(create({ isOrganizer: true, event: makeEvent({ status: 'ongoing' }) }).canCancel()).toBe(false);
  });

  it('navega para o detalhe do evento', () => {
    const c = create();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    c.goToEvent();
    expect(navigate).toHaveBeenCalledWith(['/events', 'event-1']);
  });
});
