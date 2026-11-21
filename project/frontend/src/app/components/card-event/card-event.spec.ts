import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { CardEvent } from './card-event';

describe('CardEvent', () => {
  let component: CardEvent;
  let fixture: ComponentFixture<CardEvent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CardEvent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(CardEvent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('navega para o detalhe do evento', () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture.componentRef.setInput('id', 'ev-9');

    component.goToEvent();

    expect(navigate).toHaveBeenCalledWith(['/events', 'ev-9']);
  });
});
