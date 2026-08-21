import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CarriersApiService } from '../../core/api/carriers-api.service';
import { Carrier, CarrierPayload } from '../../core/models/carrier.model';

@Component({
  selector: 'app-carriers-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './carriers-page.component.html',
  styleUrl: './carriers-page.component.scss'
})
export class CarriersPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly carriersApi = inject(CarriersApiService);

  protected readonly carriers = signal<Carrier[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly selectedCarrierId = signal<string | null>(null);
  protected readonly message = signal('');
  protected readonly activeDeliveries = computed(
    () => this.carriers().filter((carrier) => carrier.doesDelivery).length
  );
  protected readonly pickupEnabled = computed(
    () => this.carriers().filter((carrier) => carrier.doesPickup).length
  );
  protected readonly selectedCarrier = computed(
    () => this.carriers().find((carrier) => carrier.id === this.selectedCarrierId()) ?? null
  );
  protected readonly modes = ['Entrega terceirizada', 'Retirada', 'Coleta programada'];

  protected readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    contactName: ['', Validators.required],
    email: [''],
    phone: [''],
    businessHours: ['', Validators.required],
    serviceArea: ['', Validators.required],
    defaultMode: ['Entrega terceirizada', Validators.required],
    doesPickup: [true],
    doesDelivery: [true],
    notes: ['']
  });

  constructor() {
    void this.load();
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  protected newCarrier(): void {
    this.selectedCarrierId.set(null);
    this.form.reset({
      name: '',
      contactName: '',
      email: '',
      phone: '',
      businessHours: '',
      serviceArea: '',
      defaultMode: 'Entrega terceirizada',
      doesPickup: true,
      doesDelivery: true,
      notes: ''
    });
    this.message.set('');
  }

  protected selectCarrier(carrier: Carrier): void {
    this.selectedCarrierId.set(carrier.id);
    this.form.reset({
      name: carrier.name,
      contactName: carrier.contactName,
      email: carrier.email,
      phone: carrier.phone,
      businessHours: carrier.businessHours,
      serviceArea: carrier.serviceArea,
      defaultMode: carrier.defaultMode,
      doesPickup: carrier.doesPickup,
      doesDelivery: carrier.doesDelivery,
      notes: carrier.notes
    });
    this.message.set(`Editando ${carrier.name}.`);
  }

  protected async save(): Promise<void> {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.message.set('');

    try {
      const payload = this.form.getRawValue() as CarrierPayload;
      const selectedCarrierId = this.selectedCarrierId();
      const carrier = selectedCarrierId
        ? await firstValueFrom(this.carriersApi.update(selectedCarrierId, payload))
        : await firstValueFrom(this.carriersApi.create(payload));

      const carriers = [...this.carriers().filter((item) => item.id !== carrier.id), carrier].sort((left, right) =>
        left.name.localeCompare(right.name)
      );

      this.carriers.set(carriers);
      this.selectedCarrierId.set(carrier.id);
      this.message.set(selectedCarrierId ? `Transportadora ${carrier.name} atualizada.` : `Transportadora ${carrier.name} criada.`);
    } finally {
      this.saving.set(false);
    }
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const carriers = await firstValueFrom(this.carriersApi.list());
      this.carriers.set(carriers);
      if (!this.selectedCarrierId()) {
        this.newCarrier();
      }
    } finally {
      this.loading.set(false);
    }
  }
}
