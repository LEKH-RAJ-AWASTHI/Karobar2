import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { LedgerService } from '../services/ledger.service';
import { LEDGER_TYPES, LedgerType } from '../models/ledger.model';

@Component({
  selector: 'app-ledger-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './ledger-form.html',
  styleUrls: ['./ledger-form.css']
})
export class LedgerForm implements OnInit {
  private fb = inject(FormBuilder);
  private ledgerService = inject(LedgerService);
  private router = inject(Router);

  readonly ledgerTypes = LEDGER_TYPES;
  loading = signal(false);
  errorMessage = signal('');
  successMessage = signal('');

  form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200), Validators.minLength(2)]],
    type: ['', Validators.required],
    openingBalance: [0, [Validators.required, Validators.min(0), Validators.max(999999999)]],
    balanceType: ['Receivable', Validators.required],
    openingBalanceDate: [new Date().toISOString().substring(0, 10), Validators.required]
  });

  ngOnInit(): void {}

  get selectedType(): LedgerType | '' {
    return this.form.get('type')?.value || '';
  }

  get typeHint(): string {
    const hints: Record<LedgerType, string> = {
      Farmer:   'Used for farmer loan accounts and crop advance tracking.',
      Bank:     'Represents a bank account for cash flow management.',
      Party:    'Used for buyer/seller trading partner accounts.',
      Product:  'Tracks inventory movements and product-level costing.',
      Expense:  'Records operational costs and business expenditures.',
      Interest: 'Tracks interest income/expense on loans.',
      Equity:   'Represents owner capital or system adjustment accounts.'
    };
    return this.selectedType ? hints[this.selectedType as LedgerType] : '';
  }

  getTypeIcon(type: string): string {
    const map: Record<string, string> = {
      Farmer: 'agriculture', Bank: 'account_balance', Party: 'handshake',
      Product: 'inventory_2', Expense: 'receipt_long', Interest: 'percent', Equity: 'account_balance_wallet'
    };
    return map[type] ?? 'account_tree';
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.errorMessage.set('');
    const { name, type, openingBalance, balanceType, openingBalanceDate } = this.form.value;
    this.ledgerService.createLedger(
      name.trim(), 
      type as LedgerType, 
      openingBalance, 
      balanceType,
      openingBalanceDate
    ).subscribe({
      next: () => {
        this.successMessage.set('Ledger created successfully!');
        setTimeout(() => this.router.navigate(['/ledgers']), 800);
      },
      error: (err: any) => {
        this.loading.set(false);
        this.errorMessage.set(err?.error?.message || err?.error?.title || 'Failed to create ledger. Please try again.');
      }
    });
  }

  isFieldInvalid(field: string): boolean {
    const ctrl = this.form.get(field);
    return !!(ctrl && ctrl.invalid && ctrl.touched);
  }
}
