import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateOrderPayload, OrderDetail, OrderListItem } from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class OrdersApiService {
  private readonly http = inject(HttpClient);

  list(): Observable<OrderListItem[]> {
    return this.http.get<OrderListItem[]>('/api/orders', { withCredentials: true });
  }

  getById(orderId: string): Observable<OrderDetail> {
    return this.http.get<OrderDetail>(`/api/orders/${orderId}`, { withCredentials: true });
  }

  create(payload: CreateOrderPayload): Observable<OrderDetail> {
    return this.http.post<OrderDetail>('/api/orders', payload, { withCredentials: true });
  }

  approve(orderId: string): Observable<OrderDetail> {
    return this.http.post<OrderDetail>(`/api/orders/${orderId}/approve`, {}, { withCredentials: true });
  }

  uploadAttachment(orderId: string, file: File): Observable<OrderDetail> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<OrderDetail>(`/api/orders/${orderId}/attachments`, formData, {
      withCredentials: true
    });
  }
}
