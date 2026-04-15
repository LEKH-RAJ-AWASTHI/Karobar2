import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LedgerDetail } from './ledger-detail';

describe('LedgerDetail', () => {
  let component: LedgerDetail;
  let fixture: ComponentFixture<LedgerDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LedgerDetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LedgerDetail);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
