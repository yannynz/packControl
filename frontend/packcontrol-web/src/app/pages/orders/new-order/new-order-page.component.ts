import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AssetsApiService } from '../../../core/api/assets-api.service';
import { CarriersApiService } from '../../../core/api/carriers-api.service';
import { CustomersApiService } from '../../../core/api/customers-api.service';
import { OrdersApiService } from '../../../core/api/orders-api.service';
import { ProductsApiService } from '../../../core/api/products-api.service';
import { TechnicalAsset } from '../../../core/models/asset.model';
import { Carrier } from '../../../core/models/carrier.model';
import { Customer, CustomerPayload } from '../../../core/models/customer.model';
import { CreateOrderPayload, OrderDetail } from '../../../core/models/order.model';
import { ProductTemplate } from '../../../core/models/product.model';
import {
  orderStatusLabels,
  scopeCategoryLabels,
  serviceTypeLabels,
  technicalAnalysisStatusLabels,
  urgencyLabels
} from '../../../core/ui/order-labels';

@Component({
  selector: 'app-new-order-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DatePipe, RouterLink, CurrencyPipe],
  templateUrl: './new-order-page.component.html',
  styleUrl: './new-order-page.component.scss'
})
export class NewOrderPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly assetsApi = inject(AssetsApiService);
  private readonly customersApi = inject(CustomersApiService);
  private readonly carriersApi = inject(CarriersApiService);
  private readonly productsApi = inject(ProductsApiService);
  private readonly ordersApi = inject(OrdersApiService);

  protected readonly customers = signal<Customer[]>([]);
  protected readonly assets = signal<TechnicalAsset[]>([]);
  protected readonly carriers = signal<Carrier[]>([]);
  protected readonly products = signal<ProductTemplate[]>([]);
  protected readonly createdOrder = signal<OrderDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly customerSaving = signal(false);
  protected readonly uploading = signal(false);
  protected readonly message = signal('');

  protected readonly serviceTypes = [
    { value: 'New', label: serviceTypeLabels['New'], helper: 'Atendimento inedito ou novo projeto.' },
    { value: 'Repeat', label: serviceTypeLabels['Repeat'], helper: 'Reaproveita uma referencia ja existente.' },
    { value: 'Maintenance', label: serviceTypeLabels['Maintenance'], helper: 'Atende manutencao corretiva ou preventiva.' },
    { value: 'Rework', label: serviceTypeLabels['Rework'], helper: 'Refaz parte do conjunto por necessidade tecnica.' },
    { value: 'Adaptation', label: serviceTypeLabels['Adaptation'], helper: 'Ajusta uma solucao existente para novo uso.' }
  ] as const;
  protected readonly urgencyLevels = [
    { value: 'Normal', label: urgencyLabels['Normal'] },
    { value: 'Urgent', label: urgencyLabels['Urgent'] },
    { value: 'MachineStop', label: urgencyLabels['MachineStop'] }
  ] as const;
  protected readonly categories = [
    { value: 'produto_principal', label: scopeCategoryLabels['produto_principal'] },
    { value: 'componente', label: scopeCategoryLabels['componente'] },
    { value: 'acessorio', label: scopeCategoryLabels['acessorio'] },
    { value: 'servico', label: scopeCategoryLabels['servico'] },
    { value: 'manutencao', label: scopeCategoryLabels['manutencao'] },
    { value: 'adaptacao', label: scopeCategoryLabels['adaptacao'] }
  ] as const;
  protected readonly deliveryModes = ['Entrega propria', 'Entrega terceirizada', 'Retirada'];
  protected readonly hasCreatedOrder = computed(() => !!this.createdOrder());
  protected readonly selectedCustomer = computed(
    () => this.customers().find((customer) => customer.id === this.form.controls.customerId.value) ?? null
  );
  protected readonly availableAssets = computed(() =>
    this.assets().filter((asset) => asset.customerId === this.form.controls.customerId.value)
  );

  protected readonly form = this.fb.nonNullable.group({
    customerId: ['', Validators.required],
    assetId: [''],
    serviceType: this.fb.control<(typeof this.serviceTypes)[number]['value']>('New', {
      nonNullable: true,
      validators: [Validators.required]
    }),
    urgency: this.fb.control<(typeof this.urgencyLevels)[number]['value']>('Normal', {
      nonNullable: true,
      validators: [Validators.required]
    }),
    contextSummary: [''],
    legacyAssetReference: [''],
    notes: [''],
    scopeItems: this.fb.array([this.createScopeItemForm()])
  });

  protected readonly customerForm = this.fb.nonNullable.group({
    name: ['', Validators.required],
    documentNumber: [''],
    contactName: [''],
    email: [''],
    phone: [''],
    nicknamesText: [''],
    defaultCarrierId: [''],
    defaultDeliveryMode: ['Entrega propria'],
    postalCode: [''],
    street: [''],
    streetNumber: [''],
    district: [''],
    city: [''],
    state: [''],
    complement: [''],
    referencePoint: [''],
    notes: [''],
    score: [70, Validators.required]
  });

  constructor() {
    void this.loadBaseData();
  }

  protected get scopeItems(): FormArray {
    return this.form.controls.scopeItems;
  }

  protected setServiceType(value: (typeof this.serviceTypes)[number]['value']): void {
    this.form.controls.serviceType.setValue(value);
  }

  protected setUrgency(value: (typeof this.urgencyLevels)[number]['value']): void {
    this.form.controls.urgency.setValue(value);
  }

  protected formatStatus(value: string): string {
    return orderStatusLabels[value] ?? value;
  }

  protected formatAnalysisStatus(value: string): string {
    return technicalAnalysisStatusLabels[value] ?? value;
  }

  protected addScopeItem(): void {
    this.scopeItems.push(this.createScopeItemForm());
  }

  protected removeScopeItem(index: number): void {
    if (this.scopeItems.length > 1) {
      this.scopeItems.removeAt(index);
    }
  }

  protected onProductChange(index: number): void {
    const group = this.scopeItems.at(index);
    const productTemplateId = group.get('productTemplateId')?.value as string;
    const product = this.products().find((item) => item.id === productTemplateId);
    const pricingRule = this.selectedCustomer()?.productPricingRules.find((rule) => rule.productTemplateId === productTemplateId);
    if (!product) {
      return;
    }

    group.patchValue({
      title: group.get('title')?.value || product.name,
      category: product.category,
      productName: product.name,
      billingMethod: pricingRule?.billingMethod || product.billingMethod,
      unitPrice: pricingRule?.unitPrice ?? product.defaultUnitPrice
    });
  }

  protected onCustomerChange(): void {
    const selectedAssetId = this.form.controls.assetId.value;
    if (selectedAssetId && !this.availableAssets().some((asset) => asset.id === selectedAssetId)) {
      this.form.controls.assetId.setValue('');
      this.form.controls.legacyAssetReference.setValue('');
    }

    for (let index = 0; index < this.scopeItems.length; index++) {
      const productTemplateId = this.scopeItems.at(index).get('productTemplateId')?.value;
      if (productTemplateId) {
        this.onProductChange(index);
      }
    }
  }

  protected onAssetChange(): void {
    const asset = this.availableAssets().find((item) => item.id === this.form.controls.assetId.value);
    this.form.controls.legacyAssetReference.setValue(asset ? `${asset.code} · ${asset.alias}` : '');
  }

  protected estimatedTotal(): number {
    const items = this.scopeItems.getRawValue() as Array<{ quantity: number; unitPrice?: number | null }>;
    return items.reduce((sum, item) => sum + Number(item.quantity || 0) * Number(item.unitPrice || 0), 0);
  }

  protected async createCustomer(): Promise<void> {
    if (this.customerForm.invalid || this.customerSaving()) {
      this.customerForm.markAllAsTouched();
      return;
    }

    this.customerSaving.set(true);

    try {
      const customer = await firstValueFrom(this.customersApi.create(this.buildCustomerPayload()));
      const customers = [...this.customers(), customer].sort((left, right) => left.name.localeCompare(right.name));
      this.customers.set(customers);
      this.form.controls.customerId.setValue(customer.id);
      this.onCustomerChange();
      this.resetCustomerForm();
      this.message.set(`Cliente ${customer.name} criado e selecionado.`);
    } finally {
      this.customerSaving.set(false);
    }
  }

  protected async submit(): Promise<void> {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.message.set('');

    try {
      const payload = this.buildOrderPayload();
      const order = await firstValueFrom(this.ordersApi.create(payload));
      this.createdOrder.set(order);
      this.message.set(`Pedido ${order.number} criado com sucesso.`);
    } catch {
      this.message.set('Nao foi possivel criar o pedido.');
    } finally {
      this.saving.set(false);
    }
  }

  protected async uploadAttachment(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    const order = this.createdOrder();

    if (!file || !order || this.uploading()) {
      return;
    }

    this.uploading.set(true);

    try {
      const updatedOrder = await firstValueFrom(this.ordersApi.uploadAttachment(order.id, file));
      this.createdOrder.set(updatedOrder);
      this.message.set(`Arquivo ${file.name} anexado ao pedido ${updatedOrder.number}.`);
      input.value = '';
    } catch {
      this.message.set('Falha ao anexar o arquivo.');
    } finally {
      this.uploading.set(false);
    }
  }

  private createScopeItemForm() {
    return this.fb.nonNullable.group({
      title: ['', Validators.required],
      category: ['produto_principal', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      productTemplateId: [''],
      productName: [''],
      billingMethod: ['Por unidade'],
      unitPrice: [0],
      notes: ['']
    });
  }

  private buildCustomerPayload(): CustomerPayload {
    const raw = this.customerForm.getRawValue();
    const carrier = this.carriers().find((item) => item.id === raw.defaultCarrierId);

    return {
      name: raw.name,
      documentNumber: raw.documentNumber || null,
      contactName: raw.contactName || null,
      email: raw.email || null,
      phone: raw.phone || null,
      notes: raw.notes || null,
      nicknames: raw.nicknamesText
        .split(',')
        .map((item) => item.trim())
        .filter((item) => item.length > 0),
      postalCode: raw.postalCode || null,
      street: raw.street || null,
      streetNumber: raw.streetNumber || null,
      district: raw.district || null,
      city: raw.city || null,
      state: raw.state || null,
      cityIbgeCode: null,
      stateRegistration: null,
      taxpayerIndicator: 'NaoContribuinte',
      complement: raw.complement || null,
      referencePoint: raw.referencePoint || null,
      defaultCarrierId: raw.defaultCarrierId || null,
      defaultCarrierName: carrier?.name ?? null,
      defaultDeliveryMode: raw.defaultDeliveryMode || null,
      productPricingRules: [],
      score: Number(raw.score)
    };
  }

  private buildOrderPayload(): CreateOrderPayload {
    const raw = this.form.getRawValue();
    return {
      customerId: raw.customerId,
      serviceType: raw.serviceType,
      urgency: raw.urgency,
      contextSummary: raw.contextSummary || null,
      legacyAssetReference: raw.legacyAssetReference || null,
      notes: raw.notes || null,
      scopeItems: raw.scopeItems.map((item) => ({
        title: item.title,
        category: item.category,
        quantity: Number(item.quantity),
        productTemplateId: item.productTemplateId || null,
        productName: item.productName || null,
        billingMethod: item.billingMethod || null,
        unitPrice: Number(item.unitPrice) || null,
        notes: item.notes || null
      }))
    };
  }

  private resetCustomerForm(): void {
    this.customerForm.reset({
      name: '',
      documentNumber: '',
      contactName: '',
      email: '',
      phone: '',
      nicknamesText: '',
      defaultCarrierId: '',
      defaultDeliveryMode: 'Entrega propria',
      postalCode: '',
      street: '',
      streetNumber: '',
      district: '',
      city: '',
      state: '',
      complement: '',
      referencePoint: '',
      notes: '',
      score: 70
    });
  }

  private async loadBaseData(): Promise<void> {
    this.loading.set(true);
    try {
      const [assets, customers, carriers, products] = await Promise.all([
        firstValueFrom(this.assetsApi.list()),
        firstValueFrom(this.customersApi.list()),
        firstValueFrom(this.carriersApi.list()),
        firstValueFrom(this.productsApi.list())
      ]);

      this.assets.set(assets);
      this.customers.set(customers);
      this.carriers.set(carriers);
      this.products.set(products.filter((product) => product.active));
      if (customers[0]) {
        this.form.controls.customerId.setValue(customers[0].id);
        this.onCustomerChange();
      }
    } finally {
      this.loading.set(false);
    }
  }
}
