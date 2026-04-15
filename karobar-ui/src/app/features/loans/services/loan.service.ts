import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AddPaymentDto, CreateLoanDto, InterestCalculation, Loan, LoanEvent } from '../models/loan.model';

@Injectable({
  providedIn: 'root'
})
export class LoanService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Loans`;

  getLoans(): Observable<Loan[]> {
    return this.http.get<Loan[]>(this.apiUrl);
  }

  getLoanById(id: string): Observable<Loan & { events: LoanEvent[] }> {
    return this.http.get<Loan & { events: LoanEvent[] }>(`${this.apiUrl}/${id}`);
  }

  createLoan(loan: CreateLoanDto): Observable<Loan> {
    return this.http.post<Loan>(this.apiUrl, loan);
  }

  calculateInterest(loanId: string, calculateUntilDate: string): Observable<InterestCalculation> {
    const params = new HttpParams().set('calculateUntil', calculateUntilDate);
    return this.http.get<InterestCalculation>(`${this.apiUrl}/${loanId}/interest`, { params });
  }

  addPayment(payment: AddPaymentDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/payment`, payment);
  }
}
