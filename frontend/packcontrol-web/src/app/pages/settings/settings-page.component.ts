import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { FiscalApiService } from '../../core/api/fiscal-api.service';
import { SettingsApiService } from '../../core/api/settings-api.service';
import {
  FiscalCompanyProfileItem,
  FiscalEngineDiagnostic,
  FiscalOperationTemplate,
  FiscalOverview
} from '../../core/models/fiscal.model';
import { SettingsOverview } from '../../core/models/settings.model';
import { formatMappedLabel, userRoleLabels } from '../../core/ui/system-labels';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './settings-page.component.html',
  styleUrl: './settings-page.component.scss'
})
export class SettingsPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly settingsApi = inject(SettingsApiService);
  private readonly fiscalApi = inject(FiscalApiService);

  protected readonly overview = signal<SettingsOverview | null>(null);
  protected readonly fiscalOverview = signal<FiscalOverview | null>(null);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly diagnosing = signal(false);
  protected readonly message = signal('');
  protected readonly selectedTemplateId = signal<string | null>(null);
  protected readonly engineDiagnostic = signal<FiscalEngineDiagnostic | null>(null);
  protected readonly fiscalCompany = computed(() => this.fiscalOverview()?.companies[0] ?? null);
  protected readonly environments = ['Homologacao', 'Producao'];
  protected readonly emissionModes = ['A1', 'A3'];
  protected readonly taxRegimes = ['Lucro Presumido', 'Simples Nacional', 'Lucro Real'];
  protected readonly adapterOptions = ['mock-plugavel', 'unimake.dfe'];

  protected readonly companyForm = this.fb.nonNullable.group({
    tradeName: ['', Validators.required],
    documentNumber: ['', Validators.required],
    stateRegistration: ['', Validators.required],
    taxRegime: ['Lucro Presumido', Validators.required],
    postalCode: ['', Validators.required],
    street: ['', Validators.required],
    streetNumber: ['', Validators.required],
    district: ['', Validators.required],
    city: ['Sao Paulo', Validators.required],
    stateCode: ['SP', Validators.required],
    cityIbgeCode: ['3550308', Validators.required],
    country: ['Brasil', Validators.required],
    complement: [''],
    fiscalSeries: ['1', Validators.required],
    nfeEnabled: [true, Validators.required],
    environment: ['Homologacao', Validators.required],
    adapterName: ['mock-plugavel', Validators.required],
    certificateType: ['A1/A3', Validators.required],
    certificateMedia: ['Arquivo, pendrive ou cartao', Validators.required],
    principalEmissionMode: ['A1', Validators.required],
    contingencyEmissionMode: ['A3'],
    certificateLabel: [''],
    certificateSerialNumber: [''],
    accountantValidated: [false, Validators.required],
    homologationCredentialsValidated: [false, Validators.required],
    homologationApproved: [false, Validators.required],
    productionCredentialsValidated: [false, Validators.required],
    productionApproved: [false, Validators.required],
    onboardingNotes: ['']
  });

  protected readonly templateForm = this.fb.nonNullable.group({
    companyProfileId: [''],
    name: ['', Validators.required],
    natureOfOperation: ['Venda de produto', Validators.required],
    cfop: ['5101', Validators.required],
    finality: ['Normal', Validators.required],
    active: [true, Validators.required],
    notes: ['']
  });

  constructor() {
    void this.load();
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  protected formatRole(value: string): string {
    return formatMappedLabel(value, userRoleLabels);
  }

  protected formatStatusChipClass(company: FiscalCompanyProfileItem | null): string {
    if (!company) {
      return 'chip';
    }

    if (company.canGoLive) {
      return 'chip chip-success';
    }

    if (company.canIssueInCurrentEnvironment) {
      return 'chip chip-warning';
    }

    return 'chip chip-muted';
  }

  protected editTemplate(template: FiscalOperationTemplate): void {
    this.selectedTemplateId.set(template.id);
    this.templateForm.reset({
      companyProfileId: template.companyProfileId ?? this.fiscalCompany()?.id ?? '',
      name: template.name,
      natureOfOperation: template.natureOfOperation,
      cfop: template.cfop,
      finality: template.finality,
      active: template.active,
      notes: template.notes ?? ''
    });
    this.message.set(`Editando template ${template.name}.`);
  }

  protected newTemplate(): void {
    this.selectedTemplateId.set(null);
    this.templateForm.reset({
      companyProfileId: this.fiscalCompany()?.id ?? '',
      name: '',
      natureOfOperation: 'Venda de produto',
      cfop: '5101',
      finality: 'Normal',
      active: true,
      notes: ''
    });
  }

  protected async saveCompany(): Promise<void> {
    const company = this.fiscalCompany();
    if (!company || this.companyForm.invalid || this.saving()) {
      this.companyForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.message.set('');

    try {
      const raw = this.companyForm.getRawValue();
      const fiscalOverview = await firstValueFrom(this.fiscalApi.updateCompany(company.id, {
        tradeName: raw.tradeName,
        documentNumber: raw.documentNumber,
        stateRegistration: raw.stateRegistration,
        taxRegime: raw.taxRegime,
        postalCode: raw.postalCode,
        street: raw.street,
        streetNumber: raw.streetNumber,
        district: raw.district,
        city: raw.city,
        stateCode: raw.stateCode,
        cityIbgeCode: raw.cityIbgeCode,
        country: raw.country,
        complement: raw.complement || null,
        fiscalSeries: raw.fiscalSeries,
        nfeEnabled: raw.nfeEnabled,
        environment: raw.environment,
        adapterName: raw.adapterName,
        certificateType: raw.certificateType,
        certificateMedia: raw.certificateMedia,
        principalEmissionMode: raw.principalEmissionMode,
        contingencyEmissionMode: raw.contingencyEmissionMode || null,
        certificateLabel: raw.certificateLabel || null,
        certificateSerialNumber: raw.certificateSerialNumber || null,
        accountantValidated: raw.accountantValidated,
        homologationCredentialsValidated: raw.homologationCredentialsValidated,
        homologationApproved: raw.homologationApproved,
        productionCredentialsValidated: raw.productionCredentialsValidated,
        productionApproved: raw.productionApproved,
        onboardingNotes: raw.onboardingNotes || null
      }));

      this.applyFiscalOverview(fiscalOverview);
      this.message.set('Perfil fiscal atualizado.');
    } finally {
      this.saving.set(false);
    }
  }

  protected async runEngineDiagnostic(): Promise<void> {
    const company = this.fiscalCompany();
    if (!company || this.diagnosing()) {
      return;
    }

    this.diagnosing.set(true);
    this.message.set('');

    try {
      const diagnostic = await firstValueFrom(this.fiscalApi.getEngineDiagnostic(company.id));
      this.engineDiagnostic.set(diagnostic);
      this.message.set('Diagnostico fiscal atualizado.');
    } finally {
      this.diagnosing.set(false);
    }
  }

  protected async saveTemplate(): Promise<void> {
    if (this.templateForm.invalid || this.saving()) {
      this.templateForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.message.set('');

    try {
      const raw = this.templateForm.getRawValue();
      const templateId = this.selectedTemplateId();
      const fiscalOverview = await firstValueFrom(
        templateId
          ? this.fiscalApi.updateTemplate(templateId, {
              companyProfileId: raw.companyProfileId || null,
              name: raw.name,
              natureOfOperation: raw.natureOfOperation,
              cfop: raw.cfop,
              finality: raw.finality,
              active: raw.active,
              notes: raw.notes || null
            })
          : this.fiscalApi.createTemplate({
              companyProfileId: raw.companyProfileId || null,
              name: raw.name,
              natureOfOperation: raw.natureOfOperation,
              cfop: raw.cfop,
              finality: raw.finality,
              active: raw.active,
              notes: raw.notes || null
            })
      );

      this.applyFiscalOverview(fiscalOverview);
      this.newTemplate();
      this.message.set(templateId ? 'Template fiscal atualizado.' : 'Template fiscal criado.');
    } finally {
      this.saving.set(false);
    }
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const [overview, fiscalOverview] = await Promise.all([
        firstValueFrom(this.settingsApi.getOverview()),
        firstValueFrom(this.fiscalApi.getOverview())
      ]);
      this.overview.set(overview);
      this.applyFiscalOverview(fiscalOverview);
    } finally {
      this.loading.set(false);
    }
  }

  private applyFiscalOverview(overview: FiscalOverview): void {
    this.fiscalOverview.set(overview);
    const company = overview.companies[0];
    this.engineDiagnostic.update((current) => (current?.companyProfileId === company?.id ? current : null));
    if (company) {
      this.companyForm.reset({
        tradeName: company.tradeName,
        documentNumber: company.documentNumber,
        stateRegistration: company.stateRegistration,
        taxRegime: company.taxRegime,
        postalCode: company.postalCode,
        street: company.street,
        streetNumber: company.streetNumber,
        district: company.district,
        city: company.city,
        stateCode: company.stateCode,
        cityIbgeCode: company.cityIbgeCode,
        country: company.country,
        complement: company.complement ?? '',
        fiscalSeries: company.fiscalSeries,
        nfeEnabled: company.nfeEnabled,
        environment: company.environment,
        adapterName: company.adapterName,
        certificateType: company.certificateType,
        certificateMedia: company.certificateMedia,
        principalEmissionMode: company.principalEmissionMode,
        contingencyEmissionMode: company.contingencyEmissionMode ?? '',
        certificateLabel: company.certificateLabel ?? '',
        certificateSerialNumber: company.certificateSerialNumber ?? '',
        accountantValidated: company.accountantValidated,
        homologationCredentialsValidated: company.homologationCredentialsValidated,
        homologationApproved: company.homologationApproved,
        productionCredentialsValidated: company.productionCredentialsValidated,
        productionApproved: company.productionApproved,
        onboardingNotes: company.onboardingNotes ?? ''
      });
    }

    if (!this.selectedTemplateId()) {
      this.newTemplate();
    }
  }
}
