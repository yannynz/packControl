import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { OrdersApiService } from '../../../core/api/orders-api.service';
import { OrderDetail } from '../../../core/models/order.model';
import {
  orderStatusLabels,
  scopeCategoryLabels,
  serviceTypeLabels,
  technicalAnalysisStatusLabels,
  urgencyLabels
} from '../../../core/ui/order-labels';
import { auditEventLabels } from '../../../core/ui/system-labels';

type DetailTab = 'Resumo' | 'Arquivos' | 'Componentes' | 'OPs' | 'Logistica' | 'Historico';

@Component({
  selector: 'app-order-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe, CurrencyPipe],
  templateUrl: './order-detail-page.component.html',
  styleUrl: './order-detail-page.component.scss'
})
export class OrderDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly ordersApi = inject(OrdersApiService);

  protected readonly tabs: DetailTab[] = ['Resumo', 'Arquivos', 'Componentes', 'OPs', 'Logistica', 'Historico'];
  protected readonly activeTab = signal<DetailTab>('Resumo');
  protected readonly order = signal<OrderDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly approving = signal(false);

  protected readonly financeBalance = computed(() => {
    const order = this.order();
    if (!order) {
      return 0;
    }

    const receivables = order.financeEntries
      .filter((entry) => entry.type === 'Receber' && entry.status !== 'Liquidado')
      .reduce((total, entry) => total + entry.amount, 0);

    const payables = order.financeEntries
      .filter((entry) => entry.type === 'Pagar' && entry.status !== 'Liquidado')
      .reduce((total, entry) => total + entry.amount, 0);

    return receivables - payables;
  });

  constructor() {
    void this.load();
  }

  protected setTab(tab: DetailTab): void {
    this.activeTab.set(tab);
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  protected async approve(): Promise<void> {
    const orderId = this.orderId;
    if (!orderId || this.approving()) {
      return;
    }

    this.approving.set(true);
    try {
      const order = await firstValueFrom(this.ordersApi.approve(orderId));
      this.order.set(order);
      if (order.productionOrders.length > 0) {
        this.activeTab.set('OPs');
      }
    } finally {
      this.approving.set(false);
    }
  }

  protected get statusLabel(): string {
    const status = this.order()?.status;
    return status ? orderStatusLabels[status] ?? status : '';
  }

  protected formatServiceType(value: string): string {
    return serviceTypeLabels[value] ?? value;
  }

  protected formatUrgency(value: string): string {
    return urgencyLabels[value] ?? value;
  }

  protected formatScopeCategory(value: string): string {
    return scopeCategoryLabels[value] ?? value;
  }

  protected formatAnalysisStatus(value: string): string {
    return technicalAnalysisStatusLabels[value] ?? value;
  }

  protected formatHistoryEvent(value: string): string {
    return auditEventLabels[value] ?? value;
  }

  protected get canApprove(): boolean {
    const status = this.order()?.status;
    return status === 'Draft' || status === 'AwaitingTechnicalAnalysis' || status === 'AwaitingQuote';
  }

  private get orderId(): string {
    return this.route.snapshot.paramMap.get('orderId') ?? '';
  }

  private async load(): Promise<void> {
    const orderId = this.orderId;
    if (!orderId) {
      this.loading.set(false);
      return;
    }

    this.loading.set(true);
    try {
      const order = await firstValueFrom(this.ordersApi.getById(orderId));
      this.order.set(order);
    } finally {
      this.loading.set(false);
    }
  }
}
