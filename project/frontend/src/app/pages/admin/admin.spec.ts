import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Admin } from './admin';
import { AdminService } from '../../core/admin/admin.service';
import { AdminComment, AdminEvent, AdminReport, AdminUser } from '../../core/admin/admin.model';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/toaster/toaster.service';

const report = (overrides: Partial<AdminReport> = {}): AdminReport => ({
  id: 'r1',
  reporterId: 'u2',
  reporterName: 'Bruno',
  reportedUserId: 'u3',
  reportedUserName: 'Carlos',
  reportedUserSuspended: false,
  eventId: 'e1',
  eventTitle: 'Futebol',
  eventCanceled: false,
  reason: 1,
  reasonName: 'Comportamento inadequado',
  description: 'Foi grosseiro',
  status: 'Pending',
  createdAt: '2030-01-01T10:00:00Z',
  ...overrides,
});

const user = (overrides: Partial<AdminUser> = {}): AdminUser => ({
  id: 'u3',
  name: 'Carlos',
  email: 'carlos@email.com',
  city: 'Curitiba',
  urlPhoto: null,
  isAdmin: false,
  isSuspended: false,
  registerDate: '2029-01-01T00:00:00Z',
  eventsCreated: 2,
  reportsReceived: 1,
  ...overrides,
});

const event = (overrides: Partial<AdminEvent> = {}): AdminEvent => ({
  id: 'e1',
  title: 'Futebol',
  city: 'Curitiba',
  dateTimeStart: '2030-01-10T18:00:00Z',
  dateTimeEnd: '2030-01-10T20:00:00Z',
  canceledAt: null,
  organizerId: 'u3',
  organizerName: 'Carlos',
  categoryName: 'Esportes',
  participantsCount: 3,
  maxCapacity: 10,
  reportsCount: 0,
  status: 'upcoming',
  ...overrides,
});

const comment: AdminComment = {
  id: 'c1',
  text: 'Spam!',
  registerDate: '2030-01-01T10:00:00Z',
  userId: 'u3',
  userName: 'Carlos',
  eventId: 'e1',
  eventTitle: 'Futebol',
};

describe('Admin', () => {
  let fixture: ComponentFixture<Admin>;
  let component: Admin;
  let api: Record<string, ReturnType<typeof vi.fn>>;
  let toast: { success: ReturnType<typeof vi.fn>; error: ReturnType<typeof vi.fn> };

  const el = () => fixture.nativeElement as HTMLElement;
  const buttons = () => Array.from(el().querySelectorAll('button')) as HTMLButtonElement[];
  const button = (text: string) => buttons().find((b) => b.textContent?.trim() === text);

  async function render() {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  }

  beforeEach(async () => {
    api = {
      getStats: vi.fn(() => of({ pendingReports: 2, totalUsers: 10, suspendedUsers: 1, upcomingEvents: 4, totalEvents: 7 })),
      getReports: vi.fn(() => of([report(), report({ id: 'r2', reportedUserId: null, reportedUserName: null })])),
      getUsers: vi.fn(() => of([user(), user({ id: 'me', name: 'Eu Admin', isAdmin: true })])),
      getEvents: vi.fn(() => of([event(), event({ id: 'e2', title: 'Antigo', status: 'finished' })])),
      getComments: vi.fn(() => of([comment])),
      updateReportStatus: vi.fn(() => of(undefined)),
      setSuspended: vi.fn(() => of(undefined)),
      setAdmin: vi.fn(() => of(undefined)),
      cancelEvent: vi.fn(() => of(undefined)),
      deleteComment: vi.fn(() => of(undefined)),
    };
    toast = { success: vi.fn(), error: vi.fn() };
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    vi.spyOn(console, 'log').mockImplementation(() => {});

    await TestBed.configureTestingModule({
      imports: [Admin],
      providers: [
        provideRouter([]),
        { provide: AdminService, useValue: api },
        { provide: ToastService, useValue: toast },
        {
          provide: AuthService,
          useValue: {
            currentUser: () => ({ id: 'ME', name: 'Eu Admin', email: 'a@a.com', isAdmin: true }),
            isAdmin: () => true,
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Admin);
    component = fixture.componentInstance;
    await render();
  });

  afterEach(() => vi.restoreAllMocks());

  it('carrega o resumo e as denúncias pendentes ao abrir', () => {
    expect(api['getReports']).toHaveBeenCalledWith('Pending');
    expect(el().querySelector('#stats')?.textContent).toContain('10');
    expect(el().querySelector('#reports')?.textContent).toContain('Denúncia contra');
    expect(el().querySelector('#reports')?.textContent).toContain('Denúncia do evento');
  });

  it('filtra denúncias por status ("Todas" envia null)', () => {
    component.onReportStatusChange('');
    expect(api['getReports']).toHaveBeenLastCalledWith(null);
  });

  it('resolve uma denúncia, mostra toast e recarrega', async () => {
    button('Marcar como resolvida')!.click();
    await render();

    expect(api['updateReportStatus']).toHaveBeenCalledWith('r1', 'Resolved');
    expect(toast.success).toHaveBeenCalledWith('Denúncia marcada como resolvida.');
    expect(api['getStats']).toHaveBeenCalledTimes(2);
    expect(api['getReports']).toHaveBeenCalledTimes(2);
  });

  it('descarta e reabre denúncias', () => {
    component.updateReport(report(), 'Dismissed');
    component.updateReport(report(), 'Pending');
    expect(toast.success).toHaveBeenCalledWith('Denúncia descartada.');
    expect(toast.success).toHaveBeenCalledWith('Denúncia reaberta.');
  });

  it('suspende o usuário denunciado a partir da denúncia', () => {
    button('Suspender usuário')!.click();
    expect(api['setSuspended']).toHaveBeenCalledWith('u3', true);
  });

  it('cancela o evento denunciado a partir da denúncia', () => {
    button('Cancelar evento')!.click();
    expect(api['cancelEvent']).toHaveBeenCalledWith('e1');
  });

  it('não executa a ação quando o admin não confirma', () => {
    vi.mocked(window.confirm).mockReturnValue(false);
    component.setSuspended('u3', 'Carlos', true);
    component.cancelEvent('e1', 'Futebol');
    component.deleteComment(comment);
    component.setAdmin(user(), true);
    expect(api['setSuspended']).not.toHaveBeenCalled();
    expect(api['cancelEvent']).not.toHaveBeenCalled();
    expect(api['deleteComment']).not.toHaveBeenCalled();
    expect(api['setAdmin']).not.toHaveBeenCalled();
  });

  it('mostra a mensagem de erro do backend quando a ação falha', () => {
    api['setSuspended'].mockReturnValue(throwError(() => ({ error: { errors: ['Não pode.'] } })));
    component.setSuspended('u3', 'Carlos', true);
    expect(toast.error).toHaveBeenCalledWith('Não pode.');
    expect(component.busyId()).toBeNull();
  });

  it('usa mensagem padrão quando o erro não tem detalhes', () => {
    api['deleteComment'].mockReturnValue(throwError(() => ({})));
    component.deleteComment(comment);
    expect(toast.error).toHaveBeenCalledWith('Não foi possível concluir a ação.');
  });

  it('aba de usuários: lista, busca e não mostra ações para si mesmo', async () => {
    component.setActiveTab('users');
    await render();

    expect(api['getUsers']).toHaveBeenCalledWith('');
    const rows = el().querySelectorAll('#users tbody tr');
    expect(rows).toHaveLength(2);
    expect(rows[1].textContent).toContain('(você)');
    expect(rows[1].querySelectorAll('button')).toHaveLength(0);
    expect(component.searchPlaceholder()).toBe('Buscar por nome ou e-mail');

    component.onSearch('carl');
    expect(api['getUsers']).toHaveBeenLastCalledWith('carl');
  });

  it('aba de usuários: suspende, reativa e altera admin', () => {
    component.setActiveTab('users');
    component.setSuspended('u3', 'Carlos', true);
    component.setSuspended('u3', 'Carlos', false);
    component.setAdmin(user(), true);
    component.setAdmin(user({ isAdmin: true }), false);

    expect(api['setSuspended']).toHaveBeenCalledWith('u3', true);
    expect(api['setSuspended']).toHaveBeenCalledWith('u3', false);
    expect(api['setAdmin']).toHaveBeenCalledWith('u3', true);
    expect(api['setAdmin']).toHaveBeenCalledWith('u3', false);
    expect(toast.success).toHaveBeenCalledWith('Carlos foi reativado(a).');
    expect(toast.success).toHaveBeenCalledWith('Carlos não é mais administrador(a).');
  });

  it('aba de eventos: só permite cancelar eventos futuros ou em andamento', async () => {
    component.setActiveTab('events');
    await render();

    const rows = el().querySelectorAll('#events tbody tr');
    expect(rows[0].textContent).toContain('Próximo');
    expect(rows[0].querySelector('button')?.textContent).toContain('Cancelar');
    expect(rows[1].textContent).toContain('Encerrado');
    expect(rows[1].querySelector('button')).toBeNull();

    (rows[0].querySelector('button') as HTMLButtonElement).click();
    expect(api['cancelEvent']).toHaveBeenCalledWith('e1');
  });

  it('aba de comentários: lista e exclui', async () => {
    component.setActiveTab('comments');
    await render();

    expect(el().querySelector('#comments')?.textContent).toContain('Spam!');
    button('Excluir')!.click();
    expect(api['deleteComment']).toHaveBeenCalledWith('c1');
  });

  it('mostra erro quando a lista não carrega', async () => {
    api['getUsers'].mockReturnValue(throwError(() => new Error('x')));
    component.setActiveTab('users');
    await render();

    expect(el().textContent).toContain('Não foi possível carregar os dados.');
  });

  it('clicar na aba atual não recarrega', () => {
    component.setActiveTab('reports');
    expect(api['getReports']).toHaveBeenCalledTimes(1);
  });

  it('esconde o resumo se as estatísticas falharem', async () => {
    api['getStats'].mockReturnValue(throwError(() => new Error('x')));
    component.ngOnInit();
    await render();
    expect(el().querySelector('#stats')).toBeNull();
  });
});
