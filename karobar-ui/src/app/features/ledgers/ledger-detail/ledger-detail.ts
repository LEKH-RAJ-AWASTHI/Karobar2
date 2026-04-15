import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { LedgerService } from '../services/ledger.service';
import {
  LedgerBalanceDto,
  LedgerStatementDto,
  StatementLineDto,
  LEDGER_TYPE_LABELS,
  LedgerType
} from '../models/ledger.model';

@Component({
  selector: 'app-ledger-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './ledger-detail.html',
  styleUrl: './ledger-detail.css'
})
export class LedgerDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private ledgerService = inject(LedgerService);
  private fb = inject(FormBuilder);

  ledgerId = signal('');
  balance = signal<LedgerBalanceDto | null>(null);
  statement = signal<LedgerStatementDto | null>(null);
  loadingBalance = signal(false);
  loadingStatement = signal(false);
  balanceError = signal('');
  statementError = signal('');

  // Pagination
  currentPage = signal(1);
  readonly pageSize = 20;

  // Date filter form
  filterForm: FormGroup = this.fb.group({
    fromDate: [''],
    toDate: ['']
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') ?? '';
    this.ledgerId.set(id);
    this.loadBalance();
    this.loadStatement();
  }

  loadBalance(): void {
    this.loadingBalance.set(true);
    this.balanceError.set('');
    this.ledgerService.getBalance(this.ledgerId()).subscribe({
      next: (b) => { this.balance.set(b); this.loadingBalance.set(false); },
      error: (err: any) => {
        this.balanceError.set(err?.error?.message || 'Could not load balance.');
        this.loadingBalance.set(false);
      }
    });
  }

  loadStatement(page = 1): void {
    this.loadingStatement.set(true);
    this.statementError.set('');
    this.currentPage.set(page);
    const { fromDate, toDate } = this.filterForm.value;

    this.ledgerService.getStatement(this.ledgerId(), {
      fromDate: fromDate || undefined,
      toDate: toDate || undefined,
      page,
      pageSize: this.pageSize
    }).subscribe({
      next: (s) => { this.statement.set(s); this.loadingStatement.set(false); },
      error: (err: any) => {
        this.statementError.set(err?.error?.message || 'Could not load statement.');
        this.loadingStatement.set(false);
      }
    });
  }

  applyFilters(): void {
    this.loadStatement(1);
  }

  clearFilters(): void {
    this.filterForm.reset({ fromDate: '', toDate: '' });
    this.loadStatement(1);
  }

  // ─── Computed helpers ───────────────────────────────────────────────────────

  get ledgerName(): string { return this.balance()?.ledgerName ?? this.statement()?.ledgerName ?? '—'; }
  get totalPages(): number {
    const s = this.statement();
    return s ? Math.ceil(s.totalCount / this.pageSize) : 0;
  }

  formatAmount(val: number): string {
    const abs = Math.abs(val);
    const f = new Intl.NumberFormat('en-IN', { minimumFractionDigits: 2 }).format(abs);
    return val < 0 ? `(₹${f})` : val === 0 ? '—' : `₹${f}`;
  }

  formatCurrency(val: number): string {
    const f = new Intl.NumberFormat('en-IN', { minimumFractionDigits: 2 }).format(Math.abs(val));
    return val < 0 ? `(₹${f})` : `₹${f}`;
  }

  getBalanceClass(val: number): string {
    return val < 0 ? 'text-error' : val > 0 ? 'text-emerald-600' : 'text-on-surface-variant';
  }

  getRunningBalanceClass(val: number): string {
    return val < 0 ? 'text-error' : 'text-on-surface';
  }

  getTypeIcon(type?: string): string {
    const map: Record<string, string> = {
      Farmer: 'agriculture', Bank: 'account_balance', Party: 'handshake',
      Product: 'inventory_2', Expense: 'receipt_long', Interest: 'percent'
    };
    return type ? (map[type] ?? 'account_tree') : 'account_tree';
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.loadStatement(page);
  }
}
