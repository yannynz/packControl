import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { DashboardApiService } from '../../core/api/dashboard-api.service';
import { OrdersApiService } from '../../core/api/orders-api.service';
import { DashboardSummary } from '../../core/models/dashboard-summary.model';
import { OrderListItem } from '../../core/models/order.model';
import { orderStatusLabels } from '../../core/ui/order-labels';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss'
})
export class DashboardPageComponent {
  private readonly dashboardApi = inject(DashboardApiService);
  private readonly ordersApi = inject(OrdersApiService);

  protected readonly summary = signal<DashboardSummary | null>(null);
  protected readonly orders = signal<OrderListItem[]>([]);
  protected readonly loading = signal(true);

  constructor() {
    void this.load();
  }

  protected formatStatus(value: string): string {
    return orderStatusLabels[value] ?? value;
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const [summary, orders] = await Promise.all([
        firstValueFrom(this.dashboardApi.getSummary()),
        firstValueFrom(this.ordersApi.list())
      ]);

      this.summary.set(summary);
      this.orders.set(orders.slice(0, 6));
    } finally {
      this.loading.set(false);
    }
  }
}
