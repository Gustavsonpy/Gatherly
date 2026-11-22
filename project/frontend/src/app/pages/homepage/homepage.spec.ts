import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { Homepage } from './homepage';
import { EventService } from '../../core/event/event.service';
import { CategoryService } from '../../core/category/category.service';

describe('Homepage', () => {
  const navigate = vi.fn(() => Promise.resolve(true));

  function create(): Homepage {
    TestBed.configureTestingModule({
      imports: [Homepage],
      providers: [
        { provide: Router, useValue: { navigate } },
        { provide: EventService, useValue: {} },
        { provide: CategoryService, useValue: {} },
      ],
    });
    // Template vazio: o teste foca na lógica do componente, não na tela
    TestBed.overrideComponent(Homepage, { set: { template: '', imports: [] } });
    return TestBed.createComponent(Homepage).componentInstance;
  }

  beforeEach(() => navigate.mockClear());

  it('abre a página do evento', () => {
    create().open('e1');

    expect(navigate).toHaveBeenCalledWith(['/events', 'e1']);
  });
});
