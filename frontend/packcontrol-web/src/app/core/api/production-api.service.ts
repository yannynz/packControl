import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ProductionOverview, ProductionSectorDetail } from '../models/production.model';

@Injectable({ providedIn: 'root' })
export class ProductionApiService {
  private readonly http = inject(HttpClient);

  getOverview(): Observable<ProductionOverview> {
    return this.http.get<ProductionOverview>('/api/production/overview', { withCredentials: true });
  }

  getSector(sectorKey: string): Observable<ProductionSectorDetail> {
    return this.http.get<ProductionSectorDetail>(`/api/production/sectors/${sectorKey}`, { withCredentials: true });
  }

  advance(productionOrderId: string): Observable<ProductionOverview> {
    return this.http.post<ProductionOverview>(
      `/api/production/orders/${productionOrderId}/advance`,
      {},
      { withCredentials: true }
    );
  }

  split(productionOrderId: string, payload: {
    reason?: string | null;
    parts: Array<{ title: string; quantity: number; sector?: string | null }>;
  }): Observable<ProductionOverview> {
    return this.http.post<ProductionOverview>(
      `/api/production/orders/${productionOrderId}/split`,
      payload,
      { withCredentials: true }
    );
  }

  merge(payload: {
    productionOrderIds: string[];
    title?: string | null;
    sector?: string | null;
    reason?: string | null;
  }): Observable<ProductionOverview> {
    return this.http.post<ProductionOverview>('/api/production/orders/merge', payload, { withCredentials: true });
  }
}
