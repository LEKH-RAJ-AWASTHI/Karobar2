import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { StockSummary } from '../models/inventory.model';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Inventory`;

  getStockSummary(): Observable<StockSummary[]> {
    return this.http.get<StockSummary[]>(`${this.apiUrl}/stock-summary`);
  }
}
