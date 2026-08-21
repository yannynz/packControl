import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ProductionApiService } from '../../core/api/production-api.service';
import { ProductionOverview } from '../../core/models/production.model';

@Component({
  selector: 'app-production-page',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe, CurrencyPipe],
  templateUrl: './production-page.component.html',
  styleUrl: './production-page.component.scss'
})
export class ProductionPageComponent {
  private readonly productionApi = inject(ProductionApiService);

  protected readonly overview = signal<ProductionOverview | null>(null);
  protected readonly loading = signal(true);
  protected readonly advancingId = signal<string | null>(null);

  constructor() {
    void this.load();
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  protected async advance(productionOrderId: string): Promise<void> {
    if (this.advancingId()) {
      return;
    }

    this.advancingId.set(productionOrderId);
    try {
      const overview = await firstValueFrom(this.productionApi.advance(productionOrderId));
      this.overview.set(overview);
    } finally {
      this.advancingId.set(null);
    }
  }

  protected stars(complexity: number): string[] {
    return Array.from({ length: complexity }, (_, index) => `complexidade-${index}`);
  }

  protected sectorRoute(name: string): string[] | null {
    if (name === 'Montagem') {
      return ['/producao/montagem'];
    }

    if (name === 'Emborrachamento') {
      return ['/producao/emborrachamento'];
    }

    return null;
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const overview = await firstValueFrom(this.productionApi.getOverview());
      this.overview.set(overview);
    } finally {
      this.loading.set(false);
    }
  }
}
