import { ToastService } from './toaster.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();
    service = new ToastService();
  });

  afterEach(() => vi.useRealTimers());

  it('adiciona um toast com o título padrão do tipo', () => {
    service.success('Salvo!');

    expect(service.toasts()).toEqual([
      { id: 0, type: 'success', title: 'Concluído', message: 'Salvo!' },
    ]);
  });

  it('usa o título informado quando fornecido', () => {
    service.warning('Cuidado', 'Opa');
    expect(service.toasts()[0].title).toBe('Opa');
  });

  it('gera ids incrementais e aceita todos os tipos', () => {
    service.info('a');
    service.warning('b');
    service.error('c');

    expect(service.toasts().map((t) => [t.id, t.type, t.title])).toEqual([
      [0, 'info', 'Informação'],
      [1, 'warning', 'Atenção'],
      [2, 'error', 'Erro'],
    ]);
  });

  it('remove o toast automaticamente após a duração (4s padrão, 6s para erro)', () => {
    service.success('ok');
    service.error('falhou');

    vi.advanceTimersByTime(4000);
    expect(service.toasts().map((t) => t.type)).toEqual(['error']);

    vi.advanceTimersByTime(2000);
    expect(service.toasts()).toEqual([]);
  });

  it('dismiss remove apenas o toast indicado', () => {
    service.info('a');
    service.info('b');

    service.dismiss(0);

    expect(service.toasts().map((t) => t.message)).toEqual(['b']);
  });
});
