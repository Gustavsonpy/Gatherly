import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminService } from './admin.service';

describe('AdminService', () => {
  const base = `${environment.apiUrl}/admin`;
  let service: AdminService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AdminService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('getStats faz GET em /stats', () => {
    service.getStats().subscribe();
    expect(http.expectOne(`${base}/stats`).request.method).toBe('GET');
  });

  it('getReports envia o status só quando informado', () => {
    service.getReports('Pending').subscribe();
    expect(http.expectOne(`${base}/reports?status=Pending`).request.method).toBe('GET');

    service.getReports(null).subscribe();
    http.expectOne(`${base}/reports`);
  });

  it.each([
    ['getUsers', 'users'],
    ['getEvents', 'events'],
    ['getComments', 'comments'],
  ] as const)('%s envia a busca sem espaços e ignora busca vazia', (method, path) => {
    const call = (search: string) => (service[method] as (s: string) => Observable<unknown>)(search);

    call('  ana  ').subscribe();
    http.expectOne(`${base}/${path}?search=ana`);

    call('   ').subscribe();
    http.expectOne(`${base}/${path}`);
  });

  it('updateReportStatus faz PATCH com o status', () => {
    service.updateReportStatus('r1', 'Resolved').subscribe();
    const req = http.expectOne(`${base}/reports/r1/status`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ status: 'Resolved' });
  });

  it('setSuspended e setAdmin fazem PATCH com value', () => {
    service.setSuspended('u1', true).subscribe();
    const suspend = http.expectOne(`${base}/users/u1/suspended`);
    expect(suspend.request.method).toBe('PATCH');
    expect(suspend.request.body).toEqual({ value: true });

    service.setAdmin('u1', false).subscribe();
    const admin = http.expectOne(`${base}/users/u1/admin`);
    expect(admin.request.method).toBe('PATCH');
    expect(admin.request.body).toEqual({ value: false });
  });

  it('cancelEvent faz POST e deleteComment faz DELETE', () => {
    service.cancelEvent('e1').subscribe();
    expect(http.expectOne(`${base}/events/e1/cancel`).request.method).toBe('POST');

    service.deleteComment('c1').subscribe();
    expect(http.expectOne(`${base}/comments/c1`).request.method).toBe('DELETE');
  });
});
