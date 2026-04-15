import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { LEDGER_TYPES, LedgerType, LEDGER_TYPE_LABELS, LEDGER_TYPE_REVERSE_MAP } from '../models/ledger.model';
import { environment } from '../../../../environments/environment';

interface LedgerListItem {
  id: string;
  name: string;
  type: LedgerType;
  isActive: boolean;
  balance: number;
}

interface LedgersListDto {
  items: LedgerListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Component({
  selector: 'app-ledger-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './ledger-list.html',
  styleUrls: ['./ledger-list.css']
})
export class LedgerList implements OnInit {
  private http = inject(HttpClient);

  ledgers = signal<LedgerListItem[]>([]);
  loading = signal(false);
  errorMessage = signal('');

  searchControl = new FormControl('');
  selectedType = new FormControl<string>('');

  readonly ledgerTypes = LEDGER_TYPES;
  readonly typeLabels = LEDGER_TYPE_LABELS;

  currentPage = signal(1);
  readonly pageSize = 20;
  totalCount = signal(0);

  private readonly apiUrl = `${environment.apiUrl}/Ledgers`;

  ngOnInit(): void {
    this.loadLedgers();

    this.searchControl.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(() => { this.currentPage.set(1); this.loadLedgers(); });

    this.selectedType.valueChanges
      .subscribe(() => { this.currentPage.set(1); this.loadLedgers(); });
  }

  loadLedgers(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    let params = new HttpParams()
      .set('page', this.currentPage().toString())
      .set('pageSize', this.pageSize.toString());

    const search = this.searchControl.value?.trim();
    if (search) params = params.set('search', search);

    const type = this.selectedType.value;
    if (type) params = params.set('type', type);

    this.http.get<LedgersListDto>(this.apiUrl, { params }).subscribe({
      next: (res) => {
        this.ledgers.set(res.items ?? []);
        this.totalCount.set(res.totalCount ?? 0);
        this.loading.set(false);
      },
      error: (err: any) => {
        this.errorMessage.set(err?.error?.message || err?.error?.title || 'Failed to load ledgers. Please try again.');
        this.loading.set(false);
        this.ledgers.set([]);
      }
    });
  }

  formatBalance(val: number | null): string {
    if (val === null) return '—';
    const abs = Math.abs(val);
    const formatted = new Intl.NumberFormat('en-IN', { minimumFractionDigits: 2 }).format(abs);
    return val < 0 ? `(₹${formatted})` : `₹${formatted}`;
  }

  getTypeColor(typeInput: any): string {
    const type = this.resolveTypeName(typeInput);
    const map: Record<string, string> = {
      Farmer:   'bg-emerald-50 text-emerald-700',
      Bank:     'bg-blue-50 text-blue-700',
      Party:    'bg-violet-50 text-violet-700',
      Product:  'bg-amber-50 text-amber-700',
      Expense:  'bg-rose-50 text-rose-700',
      Interest: 'bg-cyan-50 text-cyan-700',
      Equity:   'bg-slate-100 text-slate-700',
    };
    return map[type] ?? 'bg-slate-100 text-slate-600';
  }

  getTypeIcon(typeInput: any): string {
    const type = this.resolveTypeName(typeInput);
    const map: Record<string, string> = {
      Farmer:   'agriculture',
      Bank:     'account_balance',
      Party:    'handshake',
      Product:  'inventory_2',
      Expense:  'receipt_long',
      Interest: 'percent',
      Equity:   'account_balance_wallet'
    };
    return map[type] ?? 'account_tree';
  }

  resolveTypeName(type: any): LedgerType {
    if (typeof type === 'number') {
      return LEDGER_TYPE_REVERSE_MAP[type] ?? 'Farmer';
    }
    return type as LedgerType;
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').slice(0, 2).toUpperCase();
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount() / this.pageSize);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage.set(page);
    this.loadLedgers();
  }
}
