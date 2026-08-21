import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { OrdersApiService } from '../../../core/api/orders-api.service';
import { OrderListItem } from '../../../core/models/order.model';
import { orderStatusLabels, serviceTypeLabels, urgencyLabels } from '../../../core/ui/order-labels';

@Component({
  selector: 'app-orders-list-page',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe],
  templateUrl: './orders-list-page.component.html',
  styleUrl: './orders-list-page.component.scss'
})
export class OrdersListPageComponent {
  private readonly ordersApi = inject(OrdersApiService);

  protected readonly orders = signal<OrderListItem[]>([]);
  protected readonly loading = signal(true);

  constructor() {
    void this.load();
  }

  protected formatStatus(value: string): string {
    return orderStatusLabels[value] ?? value;
  }

  protected formatServiceType(value: string): string {
    return serviceTypeLabels[value] ?? value;
  }

  protected formatUrgency(value: string): string {
    return urgencyLabels[value] ?? value;
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const orders = await firstValueFrom(this.ordersApi.list());
      this.orders.set(orders);
    } finally {
      this.loading.set(false);
    }
  }
}
