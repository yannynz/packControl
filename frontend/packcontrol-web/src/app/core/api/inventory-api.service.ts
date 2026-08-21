import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { MaterialCard, StockItem } from '../models/inventory.model';

@Injectable({ providedIn: 'root' })
export class InventoryApiService {
  private readonly http = inject(HttpClient);

  listMaterials(): Observable<MaterialCard[]> {
    return this.http.get<MaterialCard[]>('/api/materials', { withCredentials: true });
  }

  listStock(): Observable<StockItem[]> {
    return this.http.get<StockItem[]>('/api/stock', { withCredentials: true });
  }

  reserve(stockItemId: string, quantity: number): Observable<StockItem[]> {
    return this.http.post<StockItem[]>(
      `/api/stock/${stockItemId}/reserve`,
      { quantity },
      { withCredentials: true }
    );
  }

  replenish(stockItemId: string, quantity: number): Observable<StockItem[]> {
    return this.http.post<StockItem[]>(
      `/api/stock/${stockItemId}/replenish`,
      { quantity },
      { withCredentials: true }
    );
  }
}
