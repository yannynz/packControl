import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { FiscalDocument, FiscalEngineDiagnostic, FiscalOverview } from '../models/fiscal.model';

@Injectable({ providedIn: 'root' })
export class FiscalApiService {
  private readonly http = inject(HttpClient);

  getOverview(): Observable<FiscalOverview> {
    return this.http.get<FiscalOverview>('/api/fiscal/overview', { withCredentials: true });
  }

  getEngineDiagnostic(companyProfileId?: string | null): Observable<FiscalEngineDiagnostic> {
    const params = companyProfileId ? `?companyProfileId=${companyProfileId}` : '';
    return this.http.get<FiscalEngineDiagnostic>(`/api/fiscal/engine-diagnostic${params}`, { withCredentials: true });
  }

  prepare(payload: {
    financeEntryId?: string | null;
    orderId?: string | null;
    series?: string | null;
    natureOfOperation?: string | null;
    cfop?: string | null;
    notes?: string | null;
  }): Observable<FiscalDocument> {
    return this.http.post<FiscalDocument>('/api/fiscal/documents/prepare', payload, { withCredentials: true });
  }

  issue(payload: {
    fiscalDocumentId?: string | null;
    financeEntryId?: string | null;
    orderId?: string | null;
    series?: string | null;
    natureOfOperation?: string | null;
    cfop?: string | null;
    notes?: string | null;
  }): Observable<FiscalDocument> {
    return this.http.post<FiscalDocument>('/api/fiscal/documents/issue', payload, { withCredentials: true });
  }

  cancelDocument(payload: { fiscalDocumentId: string; reason: string }): Observable<FiscalDocument> {
    return this.http.post<FiscalDocument>('/api/fiscal/documents/cancel', payload, { withCredentials: true });
  }

  applyCorrectionLetter(payload: { fiscalDocumentId: string; correctionText: string }): Observable<FiscalDocument> {
    return this.http.post<FiscalDocument>('/api/fiscal/documents/correction-letter', payload, { withCredentials: true });
  }

  inutilizeNumberRange(payload: {
    companyProfileId: string;
    series: string;
    startNumber: number;
    endNumber: number;
    reason: string;
  }): Observable<FiscalOverview> {
    return this.http.post<FiscalOverview>('/api/fiscal/numbering/inutilize', payload, { withCredentials: true });
  }

  updateCompany(companyProfileId: string, payload: {
    tradeName: string;
    documentNumber: string;
    stateRegistration: string;
    taxRegime: string;
    postalCode: string;
    street: string;
    streetNumber: string;
    district: string;
    city: string;
    stateCode: string;
    cityIbgeCode: string;
    country: string;
    complement?: string | null;
    fiscalSeries: string;
    nfeEnabled: boolean;
    environment: string;
    adapterName: string;
    certificateType: string;
    certificateMedia: string;
    principalEmissionMode: string;
    contingencyEmissionMode?: string | null;
    certificateLabel?: string | null;
    certificateSerialNumber?: string | null;
    accountantValidated: boolean;
    homologationCredentialsValidated: boolean;
    homologationApproved: boolean;
    productionCredentialsValidated: boolean;
    productionApproved: boolean;
    onboardingNotes?: string | null;
  }): Observable<FiscalOverview> {
    return this.http.put<FiscalOverview>(`/api/fiscal/company-profiles/${companyProfileId}`, payload, { withCredentials: true });
  }

  createTemplate(payload: {
    companyProfileId?: string | null;
    name: string;
    natureOfOperation: string;
    cfop: string;
    finality: string;
    active: boolean;
    notes?: string | null;
  }): Observable<FiscalOverview> {
    return this.http.post<FiscalOverview>('/api/fiscal/operation-templates', payload, { withCredentials: true });
  }

  updateTemplate(templateId: string, payload: {
    companyProfileId?: string | null;
    name: string;
    natureOfOperation: string;
    cfop: string;
    finality: string;
    active: boolean;
    notes?: string | null;
  }): Observable<FiscalOverview> {
    return this.http.put<FiscalOverview>(`/api/fiscal/operation-templates/${templateId}`, payload, { withCredentials: true });
  }
}
