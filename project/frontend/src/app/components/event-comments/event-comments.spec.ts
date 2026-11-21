import { TestBed } from '@angular/core/testing';
import { EventComments } from './event-comments';
import { CommentService } from '../../core/comments/comment.service';
import { CommentModel } from '../../core/comments/comment.model';

describe('EventComments', () => {
  const comment = (id: string, userId: string): CommentModel =>
    ({ id, userId, text: `c${id}`, eventId: 'e1', registerDate: '2030-01-01T00:00:00Z',
       commentUserName: 'X', userPhoto: null }) as unknown as CommentModel;

  function create(inputs: Record<string, unknown>) {
    TestBed.configureTestingModule({
      imports: [EventComments],
      providers: [{ provide: CommentService, useValue: { createComment: vi.fn() } }],
    });
    const fixture = TestBed.createComponent(EventComments);
    fixture.componentRef.setInput('eventId', 'e1');
    for (const [k, v] of Object.entries(inputs)) fixture.componentRef.setInput(k, v);
    fixture.detectChanges();
    return fixture;
  }

  it('mostra apenas os últimos N comentários', () => {
    const comments = ['1', '2', '3', '4', '5', '6'].map((id) => comment(id, 'u'));
    const c = create({ comments, limit: 2 }).componentInstance;
    expect(c.visibleComments().map((x) => x.id)).toEqual(['5', '6']);
  });

  it('identifica o organizador ignorando maiúsculas', () => {
    const c = create({ comments: [], organizerId: 'ABC' }).componentInstance;
    expect(c.isHost(comment('1', 'abc'))).toBe(true);
    expect(c.isHost(comment('2', 'xyz'))).toBe(false);
  });

  it('emite viewAll ao clicar em "ver todos" no modo compacto', () => {
    const fixture = create({ comments: [], compact: true });
    const viewAll = vi.fn();
    fixture.componentInstance.viewAll.subscribe(viewAll);

    (fixture.nativeElement.querySelector('button') as HTMLButtonElement).click();

    expect(viewAll).toHaveBeenCalled();
  });
});
