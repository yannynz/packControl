import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { FinanceOverview } from '../models/finance.model';

@Injectable({ providedIn: 'root' })
export class FinanceApiService {
  private readonly http = inject(HttpClient);

  getOverview(): Observable<FinanceOverview> {
    return this.http.get<FinanceOverview>('/api/finance/overview', { withCredentials: true });
  }

  createEntry(payload: {
    orderId?: string | null;
    orderNumber?: string | null;
    type: string;
    description: string;
    counterparty: string;
    amount: number;
    dueAtUtc: string;
    paymentMethod: string;
    notes?: string | null;
    entrySource: string;
  }): Observable<FinanceOverview> {
    return this.http.post<FinanceOverview>('/api/finance/entries', payload, { withCredentials: true });
  }

  settle(entryId: string): Observable<FinanceOverview> {
    return this.http.post<FinanceOverview>(`/api/finance/entries/${entryId}/settle`, {}, { withCredentials: true });
  }

  generateBoleto(entryId: string): Observable<FinanceOverview> {
    return this.http.post<FinanceOverview>(`/api/finance/entries/${entryId}/boleto`, {}, { withCredentials: true });
  }

  issueInvoice(payload: {
    financeEntryId?: string | null;
    series: string;
    natureOfOperation: string;
    cfop: string;
    notes?: string | null;
  }): Observable<FinanceOverview> {
    return this.http.post<FinanceOverview>('/api/finance/invoices/issue', payload, { withCredentials: true });
  }
}
