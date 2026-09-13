import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OrangeButtonTs } from './orange-button.js';

describe('OrangeButtonTs', () => {
  let component: OrangeButtonTs;
  let fixture: ComponentFixture<OrangeButtonTs>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrangeButtonTs],
    }).compileComponents();

    fixture = TestBed.createComponent(OrangeButtonTs);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
