import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { NewEvent } from './newEvent';
import { CategoryService } from '../../core/category/category.service';
import { EventService } from '../../core/event/event.service';
import { PlaceProvider } from '../../core/places/place-provider';
import { ToastService } from '../../core/toaster/toaster.service';

describe('NewEvent', () => {
  const navigate = vi.fn(() => Promise.resolve(true));
  let eventService: Record<string, ReturnType<typeof vi.fn>>;
  let toast: { success: ReturnType<typeof vi.fn> };

  function create(): NewEvent {
    TestBed.configureTestingModule({
      imports: [NewEvent],
      providers: [
        { provide: Router, useValue: { navigate } },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null } } } },
        { provide: CategoryService, useValue: {} },
        { provide: EventService, useValue: eventService },
        { provide: PlaceProvider, useValue: {} },
        { provide: ToastService, useValue: toast },
      ],
    });
    // Template vazio: o teste foca na lógica do componente, não na tela
    TestBed.overrideComponent(NewEvent, { set: { template: '', imports: [] } });
    return TestBed.createComponent(NewEvent).componentInstance;
  }

  /** Preenche um formulário válido com imagem selecionada. */
  function fillValidForm(c: NewEvent): void {
    c.form.setValue({
      eventName: 'Futebol',
      category: 'cat-1',
      description: 'Partida amistosa',
      dateStart: '2030-01-10',
      timeStart: '10:00',
      timeEnd: '12:00',
      localization: 'Parque',
      city: 'Joinville',
      spots: 10,
    });
    c.selectedImage.set(new File(['x'], 'foto.jpg', { type: 'image/jpeg' }));
  }

  beforeEach(() => {
    navigate.mockClear();
    eventService = {
      uploadImage: vi.fn(() => of({ url: 'http://localhost/uploads/events/foto.jpg' })),
      createEvent: vi.fn(() => of({})),
      updateEvent: vi.fn(() => of({})),
    };
    toast = { success: vi.fn() };
  });

  it('cancelar volta para a lista de eventos', () => {
    create().cancel();

    expect(navigate).toHaveBeenCalledWith(['/events']);
  });

  it('cancelar a edição volta para o próprio evento', () => {
    const c = create();
    c.editingId.set('e1');

    c.cancel();

    expect(navigate).toHaveBeenCalledWith(['/events', 'e1']);
  });

  it('ao criar o evento, volta para a lista e mostra mensagem', () => {
    const c = create();
    fillValidForm(c);

    c.submit();

    expect(eventService['createEvent']).toHaveBeenCalled();
    expect(navigate).toHaveBeenCalledWith(['/events']);
    expect(toast.success).toHaveBeenCalled();
  });

  it('ao salvar a edição, volta para o próprio evento e mostra mensagem', () => {
    const c = create();
    c.editingId.set('e1');
    fillValidForm(c);

    c.submit();

    expect(eventService['updateEvent']).toHaveBeenCalledWith('e1', expect.anything());
    expect(navigate).toHaveBeenCalledWith(['/events', 'e1']);
    expect(toast.success).toHaveBeenCalled();
  });
});
