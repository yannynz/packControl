import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Carrier, CarrierPayload } from '../models/carrier.model';

@Injectable({ providedIn: 'root' })
export class CarriersApiService {
  private readonly http = inject(HttpClient);

  list(): Observable<Carrier[]> {
    return this.http.get<Carrier[]>('/api/carriers', { withCredentials: true });
  }

  create(payload: CarrierPayload): Observable<Carrier> {
    return this.http.post<Carrier>('/api/carriers', payload, { withCredentials: true });
  }

  update(carrierId: string, payload: CarrierPayload): Observable<Carrier> {
    return this.http.put<Carrier>(`/api/carriers/${carrierId}`, payload, { withCredentials: true });
  }
}
