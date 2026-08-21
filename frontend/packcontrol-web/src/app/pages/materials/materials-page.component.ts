import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { InventoryApiService } from '../../core/api/inventory-api.service';
import { MaterialCard } from '../../core/models/inventory.model';
import { formatMappedLabel, materialCategoryLabels, technicalTypeLabels } from '../../core/ui/system-labels';

@Component({
  selector: 'app-materials-page',
  standalone: true,
  imports: [CommonModule, CurrencyPipe],
  templateUrl: './materials-page.component.html',
  styleUrl: './materials-page.component.scss'
})
export class MaterialsPageComponent {
  private readonly inventoryApi = inject(InventoryApiService);

  protected readonly materials = signal<MaterialCard[]>([]);
  protected readonly loading = signal(true);

  constructor() {
    void this.load();
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  protected formatCategory(value: string): string {
    return formatMappedLabel(value, materialCategoryLabels);
  }

  protected formatTechnicalType(value: string): string {
    return formatMappedLabel(value, technicalTypeLabels);
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const materials = await firstValueFrom(this.inventoryApi.listMaterials());
      this.materials.set(materials);
    } finally {
      this.loading.set(false);
    }
  }
}
