import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-transaction-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './transaction-list.html',
  styleUrls: ['./transaction-list.css']
})
export class TransactionList {}
