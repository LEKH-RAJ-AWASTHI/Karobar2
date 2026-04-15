import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-transaction-form',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './transaction-form.html',
  styleUrls: ['./transaction-form.css']
})
export class TransactionForm {}
