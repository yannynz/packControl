import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { FinanceApiService } from '../../core/api/finance-api.service';
import { FiscalApiService } from '../../core/api/fiscal-api.service';
import { OrdersApiService } from '../../core/api/orders-api.service';
import { FinanceEntry, FinanceOverview } from '../../core/models/finance.model';
import { FiscalDocument, FiscalOverview } from '../../core/models/fiscal.model';
import { OrderListItem } from '../../core/models/order.model';

@Component({
  selector: 'app-finance-page',
  standalone: true,
  imports: [CommonModule, RouterLink, CurrencyPipe, DatePipe, ReactiveFormsModule],
  templateUrl: './finance-page.component.html',
  styleUrl: './finance-page.component.scss'
})
export class FinancePageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly financeApi = inject(FinanceApiService);
  private readonly fiscalApi = inject(FiscalApiService);
  private readonly ordersApi = inject(OrdersApiService);

  protected readonly overview = signal<FinanceOverview | null>(null);
  protected readonly fiscalOverview = signal<FiscalOverview | null>(null);
  protected readonly orders = signal<OrderListItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly actingId = signal<string | null>(null);
  protected readonly message = signal('');
  protected readonly receivableEntries = computed(
    () => this.overview()?.entries.filter((entry) => entry.type === 'Receber') ?? []
  );
  protected readonly preparedDocuments = computed(
    () => this.fiscalOverview()?.documents.filter((document) => document.status !== 'Authorized' && document.status !== 'Cancelled') ?? []
  );
  protected readonly authorizedDocuments = computed(
    () =>
      this.fiscalOverview()?.documents.filter(
        (document) => Boolean(document.accessKey) && Boolean(document.protocol) && document.status !== 'Cancelled'
      ) ?? []
  );
  protected readonly entryTypes = ['Receber', 'Pagar'];
  protected readonly paymentMethods = ['Boleto', 'Pix', 'Transferencia', 'Dinheiro', 'Compra programada'];
  protected readonly entrySources = ['Manual', 'Pedido', 'Fiscal'];

  protected readonly createForm = this.fb.nonNullable.group({
    orderId: [''],
    type: ['Receber', Validators.required],
    description: ['', Validators.required],
    counterparty: ['', Validators.required],
    amount: [0, Validators.required],
    dueDate: ['', Validators.required],
    paymentMethod: ['Boleto', Validators.required],
    notes: [''],
    entrySource: ['Manual', Validators.required]
  });

  protected readonly invoiceForm = this.fb.nonNullable.group({
    fiscalDocumentId: [''],
    financeEntryId: [''],
    series: ['1', Validators.required],
    natureOfOperation: ['Venda de produto', Validators.required],
    cfop: ['5101', Validators.required],
    notes: ['']
  });

  protected readonly cancelForm = this.fb.nonNullable.group({
    fiscalDocumentId: ['', Validators.required],
    reason: ['Cancelamento solicitado por ajuste operacional.', [Validators.required, Validators.minLength(15)]]
  });

  protected readonly correctionForm = this.fb.nonNullable.group({
    fiscalDocumentId: ['', Validators.required],
    correctionText: ['', [Validators.required, Validators.minLength(15), Validators.maxLength(1000)]]
  });

  protected readonly inutilizationForm = this.fb.nonNullable.group({
    companyProfileId: ['', Validators.required],
    series: ['1', Validators.required],
    startNumber: [0, Validators.required],
    endNumber: [0, Validators.required],
    reason: ['Inutilizacao de faixa sem uso operacional.', [Validators.required, Validators.minLength(15)]]
  });

  constructor() {
    void this.load();
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  protected async createEntry(): Promise<void> {
    if (this.createForm.invalid || this.saving()) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.message.set('');

    try {
      const raw = this.createForm.getRawValue();
      const selectedOrder = this.orders().find((order) => order.id === raw.orderId);
      const overview = await firstValueFrom(
        this.financeApi.createEntry({
          orderId: raw.orderId || null,
          orderNumber: selectedOrder?.number ?? null,
          type: raw.type,
          description: raw.description,
          counterparty: raw.counterparty,
          amount: Number(raw.amount),
          dueAtUtc: this.toIsoDate(raw.dueDate),
          paymentMethod: raw.paymentMethod,
          notes: raw.notes || null,
          entrySource: raw.entrySource
        })
      );

      this.overview.set(overview);
      this.createForm.reset({
        orderId: '',
        type: 'Receber',
        description: '',
        counterparty: '',
        amount: 0,
        dueDate: '',
        paymentMethod: 'Boleto',
        notes: '',
        entrySource: 'Manual'
      });
      this.message.set('Lancamento financeiro criado.');
    } finally {
      this.saving.set(false);
    }
  }

  protected async prepareInvoice(): Promise<void> {
    if (this.invoiceForm.invalid || this.saving()) {
      this.invoiceForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.message.set('');

    try {
      const raw = this.invoiceForm.getRawValue();
      const document = await firstValueFrom(
        this.fiscalApi.prepare({
          financeEntryId: raw.financeEntryId || null,
          orderId: null,
          series: raw.series || null,
          natureOfOperation: raw.natureOfOperation || null,
          cfop: raw.cfop || null,
          notes: raw.notes || null
        })
      );

      this.invoiceForm.patchValue({
        fiscalDocumentId: document.id
      });
      await this.load();
      this.message.set('Documento fiscal preparado na camada canônica.');
    } finally {
      this.saving.set(false);
    }
  }

  protected async issueInvoice(): Promise<void> {
    if (this.invoiceForm.invalid || this.saving()) {
      this.invoiceForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.message.set('');

    try {
      const raw = this.invoiceForm.getRawValue();
      await firstValueFrom(
        this.fiscalApi.issue({
          fiscalDocumentId: raw.fiscalDocumentId || null,
          financeEntryId: raw.fiscalDocumentId ? null : raw.financeEntryId || null,
          orderId: null,
          series: raw.series || null,
          natureOfOperation: raw.natureOfOperation || null,
          cfop: raw.cfop || null,
          notes: raw.notes || null
        })
      );

      await this.load();
      this.message.set('NF-e emitida pela camada fiscal canônica.');
    } finally {
      this.saving.set(false);
    }
  }

  protected async cancelDocument(): Promise<void> {
    if (this.cancelForm.invalid || this.saving()) {
      this.cancelForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.message.set('');

    try {
      const raw = this.cancelForm.getRawValue();
      await firstValueFrom(
        this.fiscalApi.cancelDocument({
          fiscalDocumentId: raw.fiscalDocumentId,
          reason: raw.reason
        })
      );

      await this.load();
      this.message.set('Cancelamento fiscal registrado.');
    } finally {
      this.saving.set(false);
    }
  }

  protected async applyCorrectionLetter(): Promise<void> {
    if (this.correctionForm.invalid || this.saving()) {
      this.correctionForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.message.set('');

    try {
      const raw = this.correctionForm.getRawValue();
      await firstValueFrom(
        this.fiscalApi.applyCorrectionLetter({
          fiscalDocumentId: raw.fiscalDocumentId,
          correctionText: raw.correctionText
        })
      );

      await this.load();
      this.message.set('CC-e registrada na camada fiscal.');
    } finally {
      this.saving.set(false);
    }
  }

  protected async inutilizeNumberRange(): Promise<void> {
    if (this.inutilizationForm.invalid || this.saving()) {
      this.inutilizationForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.message.set('');

    try {
      const raw = this.inutilizationForm.getRawValue();
      const overview = await firstValueFrom(
        this.fiscalApi.inutilizeNumberRange({
          companyProfileId: raw.companyProfileId,
          series: raw.series,
          startNumber: Number(raw.startNumber),
          endNumber: Number(raw.endNumber),
          reason: raw.reason
        })
      );

      this.fiscalOverview.set(overview);
      this.syncFiscalForms();
      this.message.set('Faixa fiscal inutilizada na camada canônica.');
    } finally {
      this.saving.set(false);
    }
  }

  protected useDocument(document: FiscalDocument): void {
    const canOperateFiscalEvent = Boolean(document.accessKey) && Boolean(document.protocol) && document.status !== 'Cancelled';
    this.invoiceForm.patchValue({
      fiscalDocumentId: document.id,
      financeEntryId: document.financeEntryId ?? '',
      series: document.series,
      natureOfOperation: document.natureOfOperation,
      cfop: document.cfop,
      notes: document.notes ?? ''
    });
    this.cancelForm.patchValue({
      fiscalDocumentId: canOperateFiscalEvent ? document.id : ''
    });
    this.correctionForm.patchValue({
      fiscalDocumentId: canOperateFiscalEvent ? document.id : ''
    });
    this.message.set(`Documento fiscal ${document.number || 'sem numero'} selecionado.`);
  }

  protected formatEventType(value: string): string {
    const labels: Record<string, string> = {
      prepared: 'Preparado',
      issue_requested: 'Emissao solicitada',
      authorized: 'Autorizado',
      rejected: 'Falha de emissao',
      cancel_requested: 'Cancelamento solicitado',
      cancelled: 'Cancelado',
      cancel_rejected: 'Falha no cancelamento',
      correction_letter_requested: 'CC-e solicitada',
      correction_letter_registered: 'CC-e registrada',
      correction_letter_rejected: 'Falha na CC-e'
    };

    return labels[value] ?? value;
  }

  protected async settle(entry: FinanceEntry): Promise<void> {
    await this.runAction(entry.id, () => this.financeApi.settle(entry.id));
  }

  protected async generateBoleto(entry: FinanceEntry): Promise<void> {
    await this.runAction(entry.id, () => this.financeApi.generateBoleto(entry.id));
  }

  private async runAction(entryId: string, action: () => ReturnType<FinanceApiService['getOverview']>): Promise<void> {
    this.actingId.set(entryId);
    try {
      const overview = await firstValueFrom(action());
      this.overview.set(overview);
    } finally {
      this.actingId.set(null);
    }
  }

  private toIsoDate(value: string): string {
    return new Date(`${value}T12:00:00`).toISOString();
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const [financeOverview, orders, fiscalOverview] = await Promise.all([
        firstValueFrom(this.financeApi.getOverview()),
        firstValueFrom(this.ordersApi.list()),
        firstValueFrom(this.fiscalApi.getOverview())
      ]);

      this.overview.set(financeOverview);
      this.orders.set(orders);
      this.fiscalOverview.set(fiscalOverview);
      this.syncFiscalForms();
    } finally {
      this.loading.set(false);
    }
  }

  private syncFiscalForms(): void {
    const company = this.fiscalOverview()?.companies[0];
    if (!company) {
      return;
    }

    this.invoiceForm.patchValue({
      series: this.invoiceForm.getRawValue().series || company.fiscalSeries
    });

    this.inutilizationForm.patchValue({
      companyProfileId: company.id,
      series: company.fiscalSeries
    });
  }
}
