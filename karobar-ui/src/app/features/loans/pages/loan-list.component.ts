import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LoanService } from '../services/loan.service';
import { Loan } from '../models/loan.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-loan-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './loan-list.html',
  styleUrls: ['./loan-list.css']
})
export class LoanListComponent implements OnInit {
  private loanService = inject(LoanService);
  loans: Loan[] = [];
  filteredLoans: Loan[] = [];
  loading = true;
  searchTerm = '';

  ngOnInit() {
    this.fetchLoans();
  }

  fetchLoans() {
    this.loading = true;
    this.loanService.getLoans().subscribe({
      next: (data) => {
        this.loans = data;
        this.filteredLoans = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onSearch() {
    this.filteredLoans = this.loans.filter(l => 
      l.farmerName.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }
}
