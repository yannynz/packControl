import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CarriersApiService } from '../../core/api/carriers-api.service';
import { CustomersApiService } from '../../core/api/customers-api.service';
import { ProductsApiService } from '../../core/api/products-api.service';
import { Carrier } from '../../core/models/carrier.model';
import { Customer, CustomerPayload, CustomerProductPricingRule } from '../../core/models/customer.model';
import { ProductTemplate } from '../../core/models/product.model';

@Component({
  selector: 'app-customers-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './customers-page.component.html',
  styleUrl: './customers-page.component.scss'
})
export class CustomersPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly customersApi = inject(CustomersApiService);
  private readonly carriersApi = inject(CarriersApiService);
  private readonly productsApi = inject(ProductsApiService);

  protected readonly customers = signal<Customer[]>([]);
  protected readonly carriers = signal<Carrier[]>([]);
  protected readonly products = signal<ProductTemplate[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly selectedCustomerId = signal<string | null>(null);
  protected readonly message = signal('');
  protected readonly averageScore = computed(() => {
    const customers = this.customers();
    if (customers.length === 0) {
      return 0;
    }

    const total = customers.reduce((sum, customer) => sum + customer.score, 0);
    return Math.round(total / customers.length);
  });
  protected readonly customersWithCarrier = computed(
    () => this.customers().filter((customer) => !!customer.defaultCarrierName).length
  );
  protected readonly selectedCustomer = computed(
    () => this.customers().find((customer) => customer.id === this.selectedCustomerId()) ?? null
  );
  protected readonly deliveryModes = ['Entrega propria', 'Entrega terceirizada', 'Retirada'];
  protected readonly taxpayerIndicators = ['Contribuinte', 'Isento', 'NaoContribuinte'];

  protected readonly form = this.fb.nonNullable.group({
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
    cityIbgeCode: [''],
    stateRegistration: [''],
    taxpayerIndicator: ['NaoContribuinte', Validators.required],
    complement: [''],
    referencePoint: [''],
    notes: [''],
    productPricingRules: this.fb.array([]),
    score: [70, Validators.required]
  });

  constructor() {
    void this.load();
  }

  protected get pricingRules(): FormArray {
    return this.form.controls.productPricingRules;
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  protected startCreate(): void {
    this.selectedCustomerId.set(null);
    this.form.reset({
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
      cityIbgeCode: '',
      stateRegistration: '',
      taxpayerIndicator: 'NaoContribuinte',
      complement: '',
      referencePoint: '',
      notes: '',
      productPricingRules: [],
      score: 70
    });
    this.pricingRules.clear();
    this.message.set('');
  }

  protected selectCustomer(customer: Customer): void {
    this.selectedCustomerId.set(customer.id);
    this.form.reset({
      name: customer.name,
      documentNumber: customer.documentNumber ?? '',
      contactName: customer.contactName ?? '',
      email: customer.email ?? '',
      phone: customer.phone ?? '',
      nicknamesText: customer.nicknames.join(', '),
      defaultCarrierId: customer.defaultCarrierId ?? '',
      defaultDeliveryMode: customer.defaultDeliveryMode ?? 'Entrega propria',
      postalCode: customer.postalCode ?? '',
      street: customer.street ?? '',
      streetNumber: customer.streetNumber ?? '',
      district: customer.district ?? '',
      city: customer.city ?? '',
      state: customer.state ?? '',
      cityIbgeCode: customer.cityIbgeCode ?? '',
      stateRegistration: customer.stateRegistration ?? '',
      taxpayerIndicator: customer.taxpayerIndicator,
      complement: customer.complement ?? '',
      referencePoint: customer.referencePoint ?? '',
      notes: customer.notes ?? '',
      productPricingRules: [],
      score: customer.score
    });

    this.pricingRules.clear();
    for (const rule of customer.productPricingRules) {
      this.pricingRules.push(
        this.fb.nonNullable.group({
          productTemplateId: [rule.productTemplateId, Validators.required],
          productName: [rule.productName],
          billingMethod: [rule.billingMethod, Validators.required],
          unitPrice: [rule.unitPrice, Validators.required],
          notes: [rule.notes ?? '']
        })
      );
    }

    this.message.set(`Editando ${customer.name}.`);
  }

  protected addPricingRule(): void {
    this.pricingRules.push(this.createPricingRuleForm());
  }

  protected removePricingRule(index: number): void {
    if (this.pricingRules.length > 0) {
      this.pricingRules.removeAt(index);
    }
  }

  protected onPricingProductChange(index: number): void {
    const group = this.pricingRules.at(index);
    const product = this.products().find((item) => item.id === group.get('productTemplateId')?.value);
    if (!product) {
      return;
    }

    group.patchValue({
      productName: product.name,
      billingMethod: product.billingMethod,
      unitPrice: product.defaultUnitPrice
    });
  }

  protected async save(): Promise<void> {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.message.set('');

    try {
      const payload = this.buildPayload();
      const selectedCustomerId = this.selectedCustomerId();

      const customer = selectedCustomerId
        ? await firstValueFrom(this.customersApi.update(selectedCustomerId, payload))
        : await firstValueFrom(this.customersApi.create(payload));

      const customers = [...this.customers().filter((item) => item.id !== customer.id), customer].sort((left, right) =>
        left.name.localeCompare(right.name)
      );

      this.customers.set(customers);
      this.selectedCustomerId.set(customer.id);
      this.message.set(selectedCustomerId ? `Cliente ${customer.name} atualizado.` : `Cliente ${customer.name} criado.`);
    } finally {
      this.saving.set(false);
    }
  }

  protected formatAddress(customer: Customer): string {
    const parts = [customer.street, customer.streetNumber, customer.district, customer.city, customer.state].filter((value) => !!value);
    return parts.join(', ') || 'Endereco nao informado';
  }

  private createPricingRuleForm() {
    return this.fb.nonNullable.group({
      productTemplateId: ['', Validators.required],
      productName: [''],
      billingMethod: ['Por unidade', Validators.required],
      unitPrice: [0, Validators.required],
      notes: ['']
    });
  }

  private buildPayload(): CustomerPayload {
    const raw = this.form.getRawValue();
    const selectedCarrier = this.carriers().find((carrier) => carrier.id === raw.defaultCarrierId);

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
      cityIbgeCode: raw.cityIbgeCode || null,
      stateRegistration: raw.stateRegistration || null,
      taxpayerIndicator: raw.taxpayerIndicator,
      complement: raw.complement || null,
      referencePoint: raw.referencePoint || null,
      defaultCarrierId: raw.defaultCarrierId || null,
      defaultCarrierName: selectedCarrier?.name ?? null,
      defaultDeliveryMode: raw.defaultDeliveryMode || null,
      productPricingRules: (raw.productPricingRules as CustomerProductPricingRule[])
        .map((rule) => {
          const product = this.products().find((item) => item.id === rule.productTemplateId);
          return {
            productTemplateId: rule.productTemplateId,
            productName: product?.name ?? rule.productName,
            billingMethod: rule.billingMethod,
            unitPrice: Number(rule.unitPrice),
            notes: rule.notes || null
          };
        })
        .filter((rule) => !!rule.productTemplateId && rule.unitPrice > 0),
      score: Number(raw.score)
    };
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const [customers, carriers, products] = await Promise.all([
        firstValueFrom(this.customersApi.list()),
        firstValueFrom(this.carriersApi.list()),
        firstValueFrom(this.productsApi.list())
      ]);

      this.customers.set(customers);
      this.carriers.set(carriers);
      this.products.set(products.filter((product) => product.active));

      if (!this.selectedCustomerId()) {
        this.startCreate();
      }
    } finally {
      this.loading.set(false);
    }
  }
}
