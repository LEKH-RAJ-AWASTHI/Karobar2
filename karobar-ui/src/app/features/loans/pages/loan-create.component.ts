import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { LoanService } from '../services/loan.service';

@Component({
  selector: 'app-loan-create',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './loan-create.html',
  styleUrls: ['./loan-create.css']
})
export class LoanCreateComponent {
  private fb = inject(FormBuilder);
  private loanService = inject(LoanService);
  private router = inject(Router);

  loading = false;

  loanForm = this.fb.group({
    farmerId: ['', Validators.required],
    principalAmount: [null, [Validators.required, Validators.min(1)]],
    interestRate: [null, [Validators.required, Validators.min(0)]],
    startDate: [new Date().toISOString().split('T')[0], Validators.required]
  });

  onSubmit() {
    if (this.loanForm.valid) {
      this.loading = true;
      const val = this.loanForm.value;
      this.loanService.createLoan({
        farmerId: val.farmerId!,
        principalAmount: val.principalAmount!,
        interestRate: val.interestRate!,
        startDate: val.startDate!
      }).subscribe({
        next: () => {
          this.router.navigate(['/loans']);
        },
        error: () => {
          this.loading = false;
        }
      });
    }
  }
}
