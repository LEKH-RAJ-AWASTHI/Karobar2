import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LedgerForm } from './ledger-form';

describe('LedgerForm', () => {
  let component: LedgerForm;
  let fixture: ComponentFixture<LedgerForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LedgerForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LedgerForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
