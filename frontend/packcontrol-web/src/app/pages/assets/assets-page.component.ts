import { CommonModule, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AssetsApiService } from '../../core/api/assets-api.service';
import { CustomersApiService } from '../../core/api/customers-api.service';
import { TechnicalAsset, TechnicalAssetPayload } from '../../core/models/asset.model';
import { Customer } from '../../core/models/customer.model';

@Component({
  selector: 'app-assets-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DatePipe],
  templateUrl: './assets-page.component.html',
  styleUrl: './assets-page.component.scss'
})
export class AssetsPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly assetsApi = inject(AssetsApiService);
  private readonly customersApi = inject(CustomersApiService);

  protected readonly assets = signal<TechnicalAsset[]>([]);
  protected readonly customers = signal<Customer[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly selectedAssetId = signal<string | null>(null);
  protected readonly message = signal('');
  protected readonly activeAssets = computed(() => this.assets().filter((asset) => asset.status !== 'Arquivado').length);

  protected readonly form = this.fb.nonNullable.group({
    customerId: ['', Validators.required],
    code: ['', Validators.required],
    alias: ['', Validators.required],
    assetType: ['Faca completa', Validators.required],
    status: ['Ativa', Validators.required],
    revision: ['R1', Validators.required],
    componentsText: [''],
    materialsText: [''],
    lastOrderNumber: [''],
    notes: ['']
  });

  constructor() {
    void this.load();
  }

  protected readonly selectedAsset = computed(
    () => this.assets().find((asset) => asset.id === this.selectedAssetId()) ?? null
  );

  protected async reload(): Promise<void> {
    await this.load();
  }

  protected startCreate(): void {
    this.selectedAssetId.set(null);
    this.form.reset({
      customerId: this.customers()[0]?.id ?? '',
      code: '',
      alias: '',
      assetType: 'Faca completa',
      status: 'Ativa',
      revision: 'R1',
      componentsText: '',
      materialsText: '',
      lastOrderNumber: '',
      notes: ''
    });
    this.message.set('');
  }

  protected selectAsset(asset: TechnicalAsset): void {
    this.selectedAssetId.set(asset.id);
    this.form.reset({
      customerId: asset.customerId,
      code: asset.code,
      alias: asset.alias,
      assetType: asset.assetType,
      status: asset.status,
      revision: asset.revision,
      componentsText: asset.components.join(', '),
      materialsText: asset.materials.join(', '),
      lastOrderNumber: asset.lastOrderNumber ?? '',
      notes: asset.notes
    });
    this.message.set(`Editando ${asset.code}.`);
  }

  protected async save(): Promise<void> {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    try {
      const payload = this.buildPayload();
      const selectedAssetId = this.selectedAssetId();
      const asset = selectedAssetId
        ? await firstValueFrom(this.assetsApi.update(selectedAssetId, payload))
        : await firstValueFrom(this.assetsApi.create(payload));

      const assets = [...this.assets().filter((item) => item.id !== asset.id), asset].sort((left, right) =>
        left.code.localeCompare(right.code)
      );
      this.assets.set(assets);
      this.selectedAssetId.set(asset.id);
      this.message.set(selectedAssetId ? `Ativo ${asset.code} atualizado.` : `Ativo ${asset.code} criado.`);
    } finally {
      this.saving.set(false);
    }
  }

  private buildPayload(): TechnicalAssetPayload {
    const raw = this.form.getRawValue();
    return {
      customerId: raw.customerId,
      code: raw.code,
      alias: raw.alias,
      assetType: raw.assetType,
      status: raw.status,
      revision: raw.revision,
      components: this.parseList(raw.componentsText),
      materials: this.parseList(raw.materialsText),
      lastOrderNumber: raw.lastOrderNumber || null,
      notes: raw.notes
    };
  }

  private parseList(value: string): string[] {
    return value
      .split(',')
      .map((item) => item.trim())
      .filter((item) => item.length > 0);
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const [assets, customers] = await Promise.all([
        firstValueFrom(this.assetsApi.list()),
        firstValueFrom(this.customersApi.list())
      ]);

      this.assets.set(assets);
      this.customers.set(customers);
      if (!this.selectedAssetId()) {
        this.startCreate();
      }
    } finally {
      this.loading.set(false);
    }
  }
}
