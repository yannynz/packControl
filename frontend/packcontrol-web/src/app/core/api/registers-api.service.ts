import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { RegistersOverview } from '../models/registers.model';

@Injectable({ providedIn: 'root' })
export class RegistersApiService {
  private readonly http = inject(HttpClient);

  getOverview(): Observable<RegistersOverview> {
    return this.http.get<RegistersOverview>('/api/registers/overview', { withCredentials: true });
  }

  create(payload: { groupKey: string; name: string; description?: string | null }): Observable<RegistersOverview> {
    return this.http.post<RegistersOverview>('/api/registers', payload, { withCredentials: true });
  }

  update(
    registerEntryId: string,
    payload: { name: string; description?: string | null; active: boolean }
  ): Observable<RegistersOverview> {
    return this.http.put<RegistersOverview>(`/api/registers/${registerEntryId}`, payload, { withCredentials: true });
  }
}
