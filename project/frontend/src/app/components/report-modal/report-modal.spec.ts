import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ReportModal } from './report-modal';
import { ReportService } from '../../core/report/report.service';
import { ToastService } from '../../core/toaster/toaster.service';

describe('ReportModal', () => {
  const reasons = [
    { id: 1, name: 'Spam', description: 'd', icon: 'message-spam' },
    { id: 5, name: 'Outro', description: 'd', icon: 'x-circle' },
  ];
  let reportService: { getReportReasons: ReturnType<typeof vi.fn>; createReport: ReturnType<typeof vi.fn> };
  let toast: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  function create() {
    TestBed.configureTestingModule({
      imports: [ReportModal],
      providers: [
        { provide: ReportService, useValue: reportService },
        { provide: ToastService, useValue: toast },
      ],
    });
    const fixture = TestBed.createComponent(ReportModal);
    fixture.componentRef.setInput('reportedUser', {
      id: 'u2', name: 'Beto', urlPhoto: null, userScore: 0, ratingsCount: 0,
    });
    fixture.componentRef.setInput('eventId', 'e1');
    fixture.detectChanges();
    return fixture;
  }

  beforeEach(() => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    reportService = { getReportReasons: vi.fn(() => of(reasons)), createReport: vi.fn() };
    toast = { success: vi.fn(), error: vi.fn() };
  });

  it('carrega os motivos ao iniciar', () => {
    const c = create().componentInstance;
    expect(c.reportReasons()).toEqual(reasons);
  });

  it('mostra erro quando falha ao carregar motivos', () => {
    reportService.getReportReasons.mockReturnValue(throwError(() => new Error('x')));
    const c = create().componentInstance;
    expect(c.errorMessage()).toBe('Não foi possível carregar os motivos da denúncia.');
  });

  it('torna a descrição obrigatória apenas para o motivo "Outro"', () => {
    const c = create().componentInstance;

    c.activateReason(reasons[1]);
    expect(c.isDescriptionOptional()).toBe(false);

    c.activateReason(reasons[0]);
    expect(c.isDescriptionOptional()).toBe(true);
    expect(c.selectedReason()).toBe(reasons[0]);
  });

  it('atualiza a descrição a partir do input', () => {
    const c = create().componentInstance;
    c.onDescriptionInput({ target: { value: 'texto' } } as unknown as Event);
    expect(c.description()).toBe('texto');
  });

  it('não envia sem motivo selecionado', () => {
    create().componentInstance.submitReport();
    expect(reportService.createReport).not.toHaveBeenCalled();
  });

  it('envia a denúncia e emite os eventos em caso de sucesso', () => {
    reportService.createReport.mockReturnValue(of({}));
    const c = create().componentInstance;
    const reported = vi.fn();
    const close = vi.fn();
    c.reported.subscribe(reported);
    c.closed.subscribe(close);

    c.activateReason(reasons[0]);
    c.description.set('  spam demais  ');
    c.submitReport();

    expect(reportService.createReport).toHaveBeenCalledWith({
      reportedUserId: 'u2', eventId: 'e1', reason: 1, description: 'spam demais',
    });
    expect(reported).toHaveBeenCalled();
    expect(close).toHaveBeenCalled();
    expect(toast.success).toHaveBeenCalled();
    expect(c.loading()).toBe(false);
  });

  it('mostra a mensagem do backend quando o envio falha', () => {
    reportService.createReport.mockReturnValue(
      throwError(() => ({ error: { errors: ['Você já denunciou.'] } })),
    );
    const c = create().componentInstance;

    c.activateReason(reasons[0]);
    c.submitReport();

    expect(c.errorMessage()).toBe('Você já denunciou.');
    expect(toast.error).toHaveBeenCalled();
  });

  it('usa mensagem padrão quando o backend não retorna erros', () => {
    reportService.createReport.mockReturnValue(throwError(() => ({})));
    const c = create().componentInstance;

    c.activateReason(reasons[0]);
    c.submitReport();

    expect(c.errorMessage()).toBe('Não foi possível enviar a denúncia. Tente novamente.');
  });

  it('o campo de descrição tem um label associado', () => {
    create();
    const field = document.querySelector<HTMLElement>('textarea');
    expect(field?.id).toBeTruthy();
    expect(document.querySelector(`label[for="${field!.id}"]`)).not.toBeNull();
  });

  it('cada motivo é um botão que indica se está selecionado (aria-pressed)', () => {
    const fixture = create();
    const buttons = () => Array.from(fixture.nativeElement.querySelectorAll('#reports button')) as HTMLElement[];

    expect(buttons().map((b) => b.getAttribute('aria-pressed'))).toEqual(['false', 'false']);

    buttons()[1].click();
    fixture.detectChanges();

    expect(buttons().map((b) => b.getAttribute('aria-pressed'))).toEqual(['false', 'true']);
  });
});
