import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { LogisticsOverview } from '../models/logistics.model';

@Injectable({ providedIn: 'root' })
export class LogisticsApiService {
  private readonly http = inject(HttpClient);

  getOverview(): Observable<LogisticsOverview> {
    return this.http.get<LogisticsOverview>('/api/logistics/overview', { withCredentials: true });
  }

  dispatch(shipmentId: string): Observable<LogisticsOverview> {
    return this.http.post<LogisticsOverview>(
      `/api/logistics/shipments/${shipmentId}/dispatch`,
      {},
      { withCredentials: true }
    );
  }

  markWithdrawal(shipmentId: string): Observable<LogisticsOverview> {
    return this.http.post<LogisticsOverview>(
      `/api/logistics/shipments/${shipmentId}/withdrawal`,
      {},
      { withCredentials: true }
    );
  }

  markAdverse(shipmentId: string): Observable<LogisticsOverview> {
    return this.http.post<LogisticsOverview>(
      `/api/logistics/shipments/${shipmentId}/adverse`,
      {},
      { withCredentials: true }
    );
  }
}
