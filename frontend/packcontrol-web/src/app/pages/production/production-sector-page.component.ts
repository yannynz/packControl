import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ProductionApiService } from '../../core/api/production-api.service';
import { ProductionOrderCard, ProductionSectorDetail } from '../../core/models/production.model';

@Component({
  selector: 'app-production-sector-page',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe, CurrencyPipe, ReactiveFormsModule],
  templateUrl: './production-sector-page.component.html',
  styleUrl: './production-sector-page.component.scss'
})
export class ProductionSectorPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly productionApi = inject(ProductionApiService);

  protected readonly detail = signal<ProductionSectorDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly advancingId = signal<string | null>(null);
  protected readonly splittingId = signal<string | null>(null);
  protected readonly selectedForMerge = signal<string[]>([]);
  protected readonly actingStructure = signal(false);

  protected readonly splitForm = this.fb.nonNullable.group({
    firstQuantity: [1, [Validators.required, Validators.min(1)]],
    secondQuantity: [1, [Validators.required, Validators.min(1)]],
    secondTitle: [''],
    secondSector: [''],
    reason: ['']
  });

  protected readonly mergeForm = this.fb.nonNullable.group({
    title: [''],
    sector: [''],
    reason: ['']
  });

  constructor() {
    void this.load();
  }

  protected get sectorKey(): string {
    return (this.route.snapshot.data['sectorKey'] as string) ?? '';
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  protected async advance(productionOrderId: string): Promise<void> {
    this.advancingId.set(productionOrderId);
    try {
      await firstValueFrom(this.productionApi.advance(productionOrderId));
      await this.load();
    } finally {
      this.advancingId.set(null);
    }
  }

  protected startSplit(item: ProductionOrderCard): void {
    this.splittingId.set(item.id);
    const firstQuantity = Math.max(1, item.quantity - 1);
    const secondQuantity = Math.max(1, item.quantity - firstQuantity);
    this.splitForm.reset({
      firstQuantity,
      secondQuantity,
      secondTitle: `${item.title} - parte 2`,
      secondSector: item.sector,
      reason: ''
    });
  }

  protected cancelSplit(): void {
    this.splittingId.set(null);
  }

  protected canSplit(item: ProductionOrderCard): boolean {
    return item.quantity > 1 && item.status !== 'Em producao';
  }

  protected isSelectedForMerge(productionOrderId: string): boolean {
    return this.selectedForMerge().includes(productionOrderId);
  }

  protected toggleMerge(productionOrderId: string, selected: boolean): void {
    const current = new Set(this.selectedForMerge());
    if (selected) {
      current.add(productionOrderId);
    } else {
      current.delete(productionOrderId);
    }

    this.selectedForMerge.set([...current]);
  }

  protected async submitSplit(item: ProductionOrderCard): Promise<void> {
    if (this.splitForm.invalid || this.actingStructure()) {
      this.splitForm.markAllAsTouched();
      return;
    }

    const raw = this.splitForm.getRawValue();
    if (Number(raw.firstQuantity) + Number(raw.secondQuantity) !== item.quantity) {
      this.splitForm.controls.secondQuantity.setErrors({ quantityMismatch: true });
      return;
    }

    this.actingStructure.set(true);
    try {
      await firstValueFrom(this.productionApi.split(item.id, {
        reason: raw.reason || null,
        parts: [
          { title: item.title, quantity: Number(raw.firstQuantity), sector: item.sector },
          {
            title: raw.secondTitle || `${item.title} - parte 2`,
            quantity: Number(raw.secondQuantity),
            sector: raw.secondSector || item.sector
          }
        ]
      }));

      this.splittingId.set(null);
      this.selectedForMerge.set([]);
      await this.load();
    } finally {
      this.actingStructure.set(false);
    }
  }

  protected async mergeSelected(): Promise<void> {
    const selected = this.selectedForMerge();
    if (selected.length < 2 || this.actingStructure()) {
      return;
    }

    this.actingStructure.set(true);
    try {
      const raw = this.mergeForm.getRawValue();
      await firstValueFrom(this.productionApi.merge({
        productionOrderIds: selected,
        title: raw.title || null,
        sector: raw.sector || this.sectorKey,
        reason: raw.reason || null
      }));

      this.selectedForMerge.set([]);
      this.mergeForm.reset({
        title: '',
        sector: this.sectorKey,
        reason: ''
      });
      await this.load();
    } finally {
      this.actingStructure.set(false);
    }
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const detail = await firstValueFrom(this.productionApi.getSector(this.sectorKey));
      this.detail.set(detail);
      this.mergeForm.patchValue({ sector: detail.name });
    } finally {
      this.loading.set(false);
    }
  }
}
