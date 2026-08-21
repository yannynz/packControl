import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SettingsOverview } from '../models/settings.model';

@Injectable({ providedIn: 'root' })
export class SettingsApiService {
  private readonly http = inject(HttpClient);

  getOverview(): Observable<SettingsOverview> {
    return this.http.get<SettingsOverview>('/api/settings/overview', { withCredentials: true });
  }
}
