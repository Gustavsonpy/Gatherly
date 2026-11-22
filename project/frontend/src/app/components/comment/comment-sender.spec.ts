import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { CommentSender } from './comment-sender';
import { CommentService } from '../../core/comments/comment.service';

describe('CommentSender', () => {
  let commentService: { createComment: ReturnType<typeof vi.fn> };

  function create() {
    TestBed.configureTestingModule({
      imports: [CommentSender],
      providers: [{ provide: CommentService, useValue: commentService }],
    });
    const fixture = TestBed.createComponent(CommentSender);
    fixture.componentRef.setInput('eventId', 'e1');
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  beforeEach(() => {
    commentService = { createComment: vi.fn() };
  });

  it('não envia comentário vazio', () => {
    const c = create();
    c.commentControl.setValue('   ');
    c.send();
    expect(commentService.createComment).not.toHaveBeenCalled();
  });

  it('não envia enquanto outro envio está em andamento', () => {
    const c = create();
    c.commentControl.setValue('oi');
    c.sending.set(true);
    c.send();
    expect(commentService.createComment).not.toHaveBeenCalled();
  });

  it('envia o comentário, emite o criado e limpa o campo', () => {
    const created = { id: 'c1', text: 'oi' };
    commentService.createComment.mockReturnValue(of(created));
    const c = create();
    const emitted = vi.fn();
    c.commentCreated.subscribe(emitted);

    c.commentControl.setValue('  oi  ');
    c.send();

    expect(commentService.createComment).toHaveBeenCalledWith({ text: 'oi', eventId: 'e1' });
    expect(emitted).toHaveBeenCalledWith(created);
    expect(c.commentControl.value).toBe('');
    expect(c.sending()).toBe(false);
  });

  it('libera o envio novamente quando ocorre erro', () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    commentService.createComment.mockReturnValue(throwError(() => new Error('x')));
    const c = create();

    c.commentControl.setValue('oi');
    c.send();

    expect(c.sending()).toBe(false);
    expect(c.commentControl.value).toBe('oi');
  });

  it('o campo de comentário tem um label associado', () => {
    create();
    const field = document.querySelector<HTMLElement>('textarea');
    expect(field?.id).toBeTruthy();
    expect(document.querySelector(`label[for="${field!.id}"]`)).not.toBeNull();
  });
});
