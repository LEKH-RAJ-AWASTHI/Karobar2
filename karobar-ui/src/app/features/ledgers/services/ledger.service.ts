import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreateLedgerRequest,
  LedgerBalanceDto,
  LedgerStatementDto,
  LedgerStatementParams,
  LEDGER_TYPE_MAP,
  LedgerType
} from '../models/ledger.model';

@Injectable({ providedIn: 'root' })
export class LedgerService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Ledgers`;

  /** POST /api/Ledgers — create a new ledger */
  createLedger(name: string, type: LedgerType, openingBalance: number = 0, balanceType: 'Receivable' | 'Payable' = 'Receivable', openingBalanceDate?: string): Observable<string> {
    const body: CreateLedgerRequest = { 
      name, 
      type: LEDGER_TYPE_MAP[type],
      openingBalance,
      balanceType,
      openingBalanceDate: openingBalanceDate || new Date().toISOString()
    };
    return this.http.post<string>(this.apiUrl, body);
  }

  /** GET /api/Ledgers/{ledgerId}/balance */
  getBalance(ledgerId: string): Observable<LedgerBalanceDto> {
    return this.http.get<LedgerBalanceDto>(`${this.apiUrl}/${ledgerId}/balance`);
  }

  /** GET /api/Ledgers/{ledgerId}/statement */
  getStatement(ledgerId: string, params: LedgerStatementParams): Observable<LedgerStatementDto> {
    let httpParams = new HttpParams()
      .set('page', params.page.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params.toDate)   httpParams = httpParams.set('toDate', params.toDate);

    return this.http.get<LedgerStatementDto>(`${this.apiUrl}/${ledgerId}/statement`, { params: httpParams });
  }

  /** DELETE /api/Ledgers/{ledgerId} */
  deleteLedger(ledgerId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${ledgerId}`);
  }
}
