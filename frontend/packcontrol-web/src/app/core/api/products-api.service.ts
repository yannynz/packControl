import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ProductTemplate, ProductTemplatePayload } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductsApiService {
  private readonly http = inject(HttpClient);

  list(): Observable<ProductTemplate[]> {
    return this.http.get<ProductTemplate[]>('/api/products', { withCredentials: true });
  }

  create(payload: ProductTemplatePayload): Observable<ProductTemplate> {
    return this.http.post<ProductTemplate>('/api/products', payload, { withCredentials: true });
  }

  update(productId: string, payload: ProductTemplatePayload): Observable<ProductTemplate> {
    return this.http.put<ProductTemplate>(`/api/products/${productId}`, payload, { withCredentials: true });
  }
}
