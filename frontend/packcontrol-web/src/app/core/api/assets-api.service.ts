import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { TechnicalAsset, TechnicalAssetPayload } from '../models/asset.model';

@Injectable({ providedIn: 'root' })
export class AssetsApiService {
  private readonly http = inject(HttpClient);

  list(): Observable<TechnicalAsset[]> {
    return this.http.get<TechnicalAsset[]>('/api/assets', { withCredentials: true });
  }

  create(payload: TechnicalAssetPayload): Observable<TechnicalAsset> {
    return this.http.post<TechnicalAsset>('/api/assets', payload, { withCredentials: true });
  }

  update(assetId: string, payload: TechnicalAssetPayload): Observable<TechnicalAsset> {
    return this.http.put<TechnicalAsset>(`/api/assets/${assetId}`, payload, { withCredentials: true });
  }
}
