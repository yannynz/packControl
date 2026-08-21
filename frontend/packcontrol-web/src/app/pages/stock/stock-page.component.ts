import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { InventoryApiService } from '../../core/api/inventory-api.service';
import { StockItem } from '../../core/models/inventory.model';

@Component({
  selector: 'app-stock-page',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './stock-page.component.html',
  styleUrl: './stock-page.component.scss'
})
export class StockPageComponent {
  private readonly inventoryApi = inject(InventoryApiService);

  protected readonly stockItems = signal<StockItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly actingId = signal<string | null>(null);

  constructor() {
    void this.load();
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  protected async reserve(item: StockItem): Promise<void> {
    await this.runAction(item.id, () => this.inventoryApi.reserve(item.id, 1));
  }

  protected async replenish(item: StockItem): Promise<void> {
    await this.runAction(item.id, () => this.inventoryApi.replenish(item.id, 5));
  }

  private async runAction(itemId: string, action: () => ReturnType<InventoryApiService['listStock']>): Promise<void> {
    this.actingId.set(itemId);
    try {
      const items = await firstValueFrom(action());
      this.stockItems.set(items);
    } finally {
      this.actingId.set(null);
    }
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const items = await firstValueFrom(this.inventoryApi.listStock());
      this.stockItems.set(items);
    } finally {
      this.loading.set(false);
    }
  }
}
