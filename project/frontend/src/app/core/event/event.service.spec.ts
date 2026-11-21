import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Observable } from 'rxjs';
import { EventService } from './event.service';

const API = 'http://localhost:5008/api/event';

describe('EventService', () => {
  let service: EventService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(EventService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  const cases: [string, () => Observable<unknown>, string, string][] = [
    ['getEvents', () => service.getEvents(), 'GET', API],
    ['getEventsByCity', () => service.getEventsByCity(), 'GET', `${API}/my-city`],
    ['getEventById', () => service.getEventById('1'), 'GET', `${API}/1`],
    ['joinEvent', () => service.joinEvent('1'), 'POST', `${API}/1/join`],
    ['leaveEvent', () => service.leaveEvent('1'), 'DELETE', `${API}/1/join`],
    ['getParticipants', () => service.getParticipants('1'), 'GET', `${API}/1/participants`],
    ['getMyEvents', () => service.getMyEvents(), 'GET', `${API}/mine`],
    ['getFutureEvents', () => service.getFutureEvents(), 'GET', `${API}/future-events`],
    ['cancelEvent', () => service.cancelEvent('1'), 'POST', `${API}/1/cancel`],
  ];

  it.each(cases)('%s chama %s %s', (_name, call, method, url) => {
    call().subscribe();
    const req = http.expectOne(url);
    expect(req.request.method).toBe(method);
    req.flush(null);
  });

  it('createEvent envia o corpo do evento', () => {
    const body = { title: 'Evento' } as never;
    service.createEvent(body).subscribe();

    const req = http.expectOne(`${API}/create`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toBe(body);
    req.flush({});
  });

  it('uploadImage envia o arquivo como FormData', () => {
    const file = new File(['x'], 'foto.png', { type: 'image/png' });
    service.uploadImage(file).subscribe();

    const req = http.expectOne(`${API}/upload-image`);
    expect(req.request.body).toBeInstanceOf(FormData);
    expect((req.request.body as FormData).get('file')).toBeInstanceOf(File);
    req.flush({ url: 'x' });
  });
});
