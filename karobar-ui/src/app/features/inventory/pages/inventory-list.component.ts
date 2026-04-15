import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CommonModule as AppCommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InventoryService } from '../services/inventory.service';
import { StockSummary } from '../models/inventory.model';

@Component({
  selector: 'app-inventory-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inventory-list.component.html',
  styleUrls: ['./inventory-list.component.css']
})
export class InventoryListComponent implements OnInit {
  private inventoryService = inject(InventoryService);
  
  stocks: StockSummary[] = [];
  filteredStocks: StockSummary[] = [];
  loading = true;
  searchTerm = '';

  ngOnInit() {
    this.fetchStockSummary();
  }

  fetchStockSummary() {
    this.loading = true;
    this.inventoryService.getStockSummary().subscribe({
      next: (data) => {
        this.stocks = data;
        this.filteredStocks = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onSearch() {
    this.filteredStocks = this.stocks.filter(s => 
      s.productName.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }

  onSort(field: keyof StockSummary) {
    this.filteredStocks.sort((a, b) => {
      const valA = a[field];
      const valB = b[field];
      if (typeof valA === 'string' && typeof valB === 'string') {
        return valA.localeCompare(valB);
      }
      return (valA as number) - (valB as number);
    });
  }
}
