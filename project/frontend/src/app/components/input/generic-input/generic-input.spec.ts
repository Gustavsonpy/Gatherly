import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GenericInputTs } from './generic-input';

describe('GenericInputTs', () => {
  let component: GenericInputTs;
  let fixture: ComponentFixture<GenericInputTs>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GenericInputTs],
    }).compileComponents();

    fixture = TestBed.createComponent(GenericInputTs);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
