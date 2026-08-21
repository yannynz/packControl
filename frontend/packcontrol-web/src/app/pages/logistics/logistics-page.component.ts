import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { LogisticsApiService } from '../../core/api/logistics-api.service';
import { LogisticsOverview, Shipment } from '../../core/models/logistics.model';

@Component({
  selector: 'app-logistics-page',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe],
  templateUrl: './logistics-page.component.html',
  styleUrl: './logistics-page.component.scss'
})
export class LogisticsPageComponent {
  private readonly logisticsApi = inject(LogisticsApiService);

  protected readonly overview = signal<LogisticsOverview | null>(null);
  protected readonly loading = signal(true);
  protected readonly actingId = signal<string | null>(null);

  constructor() {
    void this.load();
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  protected async dispatch(shipment: Shipment): Promise<void> {
    await this.runAction(shipment.id, () => this.logisticsApi.dispatch(shipment.id));
  }

  protected async markWithdrawal(shipment: Shipment): Promise<void> {
    await this.runAction(shipment.id, () => this.logisticsApi.markWithdrawal(shipment.id));
  }

  protected async markAdverse(shipment: Shipment): Promise<void> {
    await this.runAction(shipment.id, () => this.logisticsApi.markAdverse(shipment.id));
  }

  private async runAction(shipmentId: string, action: () => ReturnType<LogisticsApiService['getOverview']>): Promise<void> {
    this.actingId.set(shipmentId);
    try {
      const overview = await firstValueFrom(action());
      this.overview.set(overview);
    } finally {
      this.actingId.set(null);
    }
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const overview = await firstValueFrom(this.logisticsApi.getOverview());
      this.overview.set(overview);
    } finally {
      this.loading.set(false);
    }
  }
}
