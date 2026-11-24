import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { RatingUserModal } from './user-rating-modal';
import { AuthService } from '../../core/auth/auth.service';
import { RateService } from '../../core/rating/rating.service';
import { ToastService } from '../../core/toaster/toaster.service';
import { makeEvent } from '../../testing/test-helpers';

describe('RatingUserModal', () => {
  let rateService: { sendRating: ReturnType<typeof vi.fn> };
  let toast: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  function create() {
    TestBed.configureTestingModule({
      imports: [RatingUserModal],
      providers: [
        { provide: AuthService, useValue: { currentUser: signal({ id: 'u1', name: 'A', email: 'a' }) } },
        { provide: RateService, useValue: rateService },
        { provide: ToastService, useValue: toast },
      ],
    });
    const fixture = TestBed.createComponent(RatingUserModal);
    fixture.componentRef.setInput('ratedUser', {
      id: 'u2', name: 'Beto', urlPhoto: null, userScore: 4, ratingsCount: 2,
    });
    fixture.componentRef.setInput('event', makeEvent({ id: 'e1', status: 'done' }));
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  beforeEach(() => {
    vi.spyOn(console, 'log').mockImplementation(() => {});
    rateService = { sendRating: vi.fn() };
    toast = { success: vi.fn(), error: vi.fn() };
  });

  it('paintStars define a nota e o feedback textual', () => {
    const c = create();
    c.paintStars(4);
    expect(c.starValue()).toBe(4);
    expect(c.rateFeedback()).toBe('Muito bom');
  });

  it('atualiza a descrição a partir do input', () => {
    const c = create();
    c.onDescriptionInput({ target: { value: 'legal' } } as unknown as Event);
    expect(c.description()).toBe('legal');
  });

  it('envia a avaliação e emite eventos em caso de sucesso', () => {
    rateService.sendRating.mockReturnValue(of({}));
    const c = create();
    const rated = vi.fn();
    const close = vi.fn();
    c.rated.subscribe(rated);
    c.closed.subscribe(close);

    c.paintStars(5);
    c.description.set('Ótimo');
    c.submitRating();

    expect(rateService.sendRating).toHaveBeenCalledWith({
      rate: 5, description: 'Ótimo', ratedUserId: 'u2', eventId: 'e1',
    });
    expect(rated).toHaveBeenCalled();
    expect(close).toHaveBeenCalled();
    expect(toast.success).toHaveBeenCalled();
  });

  it('mostra o erro do backend quando a avaliação falha', () => {
    rateService.sendRating.mockReturnValue(throwError(() => ({ error: { errors: ['Já avaliado.'] } })));
    const c = create();

    c.submitRating();

    expect(c.errorMessage()).toBe('Já avaliado.');
    expect(toast.error).toHaveBeenCalled();
  });

  it('usa mensagem padrão quando o erro não tem detalhes', () => {
    rateService.sendRating.mockReturnValue(throwError(() => ({})));
    const c = create();

    c.submitRating();

    expect(c.errorMessage()).toBe('Não foi possível avaliar o usuário.');
  });

  it('o campo de descrição tem um label associado', () => {
    create();
    const field = document.querySelector<HTMLElement>('textarea');
    expect(field?.id).toBeTruthy();
    expect(document.querySelector(`label[for="${field!.id}"]`)).not.toBeNull();
  });
});
