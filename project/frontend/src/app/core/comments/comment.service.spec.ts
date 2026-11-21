import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CommentService } from './comment.service';
import { CategoryService } from '../category/category.service';
import { ReportService } from '../report/report.service';
import { RateService } from '../rating/rating.service';

const API = 'http://localhost:5008/api';

describe('Services HTTP simples', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  describe('CommentService', () => {
    it('getComments busca todos os comentários', () => {
      TestBed.inject(CommentService).getComments().subscribe();
      expect(http.expectOne(`${API}/comment`).request.method).toBe('GET');
    });

    it('getCommentsByEvent busca comentários do evento', () => {
      TestBed.inject(CommentService).getCommentsByEvent('ev1').subscribe();
      expect(http.expectOne(`${API}/comment/ev1`).request.method).toBe('GET');
    });

    it('createComment envia o comentário', () => {
      const data = { text: 'Olá', eventId: 'ev1' };
      TestBed.inject(CommentService).createComment(data).subscribe();
      const req = http.expectOne(`${API}/comment/create`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(data);
    });
  });

  describe('CategoryService', () => {
    it('getCategories busca as categorias', () => {
      TestBed.inject(CategoryService).getCategories().subscribe();
      expect(http.expectOne(`${API}/category`).request.method).toBe('GET');
    });
  });

  describe('ReportService', () => {
    it('getReportReasons busca os motivos', () => {
      TestBed.inject(ReportService).getReportReasons().subscribe();
      expect(http.expectOne(`${API}/report-reasons`).request.method).toBe('GET');
    });

    it('createReport envia a denúncia', () => {
      const report = { reportedUserId: 'u2', eventId: 'e1', reason: 1, description: '' };
      TestBed.inject(ReportService).createReport(report).subscribe();
      const req = http.expectOne(`${API}/report/create`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(report);
    });
  });

  describe('RateService', () => {
    it('sendRating envia a avaliação', () => {
      const rating = { rate: 5, description: null, ratedUserId: 'u2', eventId: 'e1' };
      TestBed.inject(RateService).sendRating(rating).subscribe();
      const req = http.expectOne(`${API}/rate/create`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(rating);
    });
  });
});
