import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { LoanService } from '../services/loan.service';
import { AddPaymentDto, InterestCalculation, Loan, LoanEvent } from '../models/loan.model';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-loan-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule],
  templateUrl: './loan-detail.html',
  styleUrls: ['./loan-detail.css']
})
export class LoanDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private loanService = inject(LoanService);
  private fb = inject(FormBuilder);

  loanId: string = '';
  loan: (Loan & { events: LoanEvent[] }) | null = null;
  loading = true;
  calculating = false;
  submittingPayment = false;

  calculateUntilDate = new Date().toISOString().split('T')[0];
  interestResult: InterestCalculation | null = null;

  paymentForm = this.fb.group({
    amount: [null, [Validators.required, Validators.min(1)]],
    date: [new Date().toISOString().split('T')[0], Validators.required]
  });

  ngOnInit() {
    this.route.params.pipe(
      switchMap(params => {
        this.loanId = params['id'];
        return this.loanService.getLoanById(this.loanId);
      })
    ).subscribe({
      next: (data) => {
        this.loan = data;
        this.loading = false;
        this.onCalculateInterest();
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onCalculateInterest() {
    this.calculating = true;
    this.loanService.calculateInterest(this.loanId, this.calculateUntilDate).subscribe({
      next: (res) => {
        this.interestResult = res;
        this.calculating = false;
      },
      error: () => {
        this.calculating = false;
      }
    });
  }

  onAddPayment() {
    if (this.paymentForm.valid && this.loan) {
      this.submittingPayment = true;
      const val = this.paymentForm.value;
      const payment: AddPaymentDto = {
        loanId: this.loanId,
        amount: val.amount!,
        date: val.date!
      };

      this.loanService.addPayment(payment).subscribe({
        next: () => {
          this.submittingPayment = false;
          this.paymentForm.reset({
            amount: null,
            date: new Date().toISOString().split('T')[0]
          });
          // Refresh data
          this.ngOnInit();
        },
        error: () => {
          this.submittingPayment = false;
        }
      });
    }
  }
}
