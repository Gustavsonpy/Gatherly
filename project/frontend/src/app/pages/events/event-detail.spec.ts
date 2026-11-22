import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { EventDetail } from './event-detail';
import { EventService } from '../../core/event/event.service';
import { CommentService } from '../../core/comments/comment.service';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/toaster/toaster.service';
import { makeEvent } from '../../testing/test-helpers';

describe('EventDetail', () => {
  const navigate = vi.fn(() => Promise.resolve(true));

  function create(): EventDetail {
    TestBed.configureTestingModule({
      imports: [EventDetail],
      providers: [
        { provide: Router, useValue: { navigate } },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'e1' } } } },
        { provide: EventService, useValue: {} },
        { provide: CommentService, useValue: {} },
        { provide: AuthService, useValue: { currentUser: () => null } },
        { provide: ToastService, useValue: {} },
      ],
    });
    // Template vazio: o teste foca na lógica do componente, não na tela
    TestBed.overrideComponent(EventDetail, { set: { template: '', imports: [] } });
    return TestBed.createComponent(EventDetail).componentInstance;
  }

  beforeEach(() => navigate.mockClear());

  it('editar leva para a página de edição do evento', () => {
    const c = create();
    c.event.set(makeEvent({ id: 'e1' }));

    c.editEvent();

    expect(navigate).toHaveBeenCalledWith(['/events', 'e1', 'edit']);
  });
});
