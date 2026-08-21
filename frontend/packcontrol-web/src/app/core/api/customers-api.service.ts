import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Customer, CustomerPayload } from '../models/customer.model';

@Injectable({ providedIn: 'root' })
export class CustomersApiService {
  private readonly http = inject(HttpClient);

  list(): Observable<Customer[]> {
    return this.http.get<Customer[]>('/api/customers', { withCredentials: true });
  }

  create(payload: CustomerPayload): Observable<Customer> {
    return this.http.post<Customer>('/api/customers', payload, { withCredentials: true });
  }

  update(customerId: string, payload: CustomerPayload): Observable<Customer> {
    return this.http.put<Customer>(`/api/customers/${customerId}`, payload, { withCredentials: true });
  }
}
