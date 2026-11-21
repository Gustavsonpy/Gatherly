import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { UserService } from './user.service';

const API = 'http://localhost:5008/api/user';

describe('UserService', () => {
  let service: UserService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(UserService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('create faz POST em /user/create', () => {
    const dto = { name: 'A', email: 'a@a.com', password: '123456', birthday: '2000-01-01' };
    service.create(dto).subscribe();

    const req = http.expectOne(`${API}/create`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(dto);
    req.flush({});
  });

  it('getMe faz GET em /user/me', () => {
    service.getMe().subscribe();
    expect(http.expectOne(`${API}/me`).request.method).toBe('GET');
  });

  it('updateMe faz PUT em /user/me', () => {
    service.updateMe({ name: 'A', birthday: '2000-01-01', description: null, city: null }).subscribe();
    expect(http.expectOne(`${API}/me`).request.method).toBe('PUT');
  });

  it('uploadPhoto envia FormData para /user/me/photo', () => {
    service.uploadPhoto(new File(['x'], 'a.png')).subscribe();
    const req = http.expectOne(`${API}/me/photo`);
    expect(req.request.body).toBeInstanceOf(FormData);
  });

  it('getUserById faz GET em /user/:id', () => {
    service.getUserById('42').subscribe();
    expect(http.expectOne(`${API}/42`).request.method).toBe('GET');
  });
});
