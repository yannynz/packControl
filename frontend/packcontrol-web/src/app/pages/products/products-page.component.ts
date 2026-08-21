import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { InventoryApiService } from '../../core/api/inventory-api.service';
import { ProductsApiService } from '../../core/api/products-api.service';
import { MaterialCard } from '../../core/models/inventory.model';
import { ProductTemplate, ProductTemplatePayload } from '../../core/models/product.model';
import { scopeCategoryLabels } from '../../core/ui/order-labels';

@Component({
  selector: 'app-products-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CurrencyPipe],
  templateUrl: './products-page.component.html',
  styleUrl: './products-page.component.scss'
})
export class ProductsPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly productsApi = inject(ProductsApiService);
  private readonly inventoryApi = inject(InventoryApiService);

  protected readonly products = signal<ProductTemplate[]>([]);
  protected readonly materials = signal<MaterialCard[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly selectedProductId = signal<string | null>(null);
  protected readonly message = signal('');
  protected readonly activeProducts = computed(() => this.products().filter((product) => product.active).length);
  protected readonly selectedProduct = computed(
    () => this.products().find((product) => product.id === this.selectedProductId()) ?? null
  );
  protected readonly categories = Object.entries(scopeCategoryLabels).map(([value, label]) => ({ value, label }));
  protected readonly billingMethods = ['Por unidade', 'Por milheiro', 'Por hora tecnica', 'Por metro aplicado'];
  protected readonly sectors = ['Preparacao', 'Corte', 'Montagem', 'Emborrachamento', 'Expedicao'];
  protected readonly fiscalUnits = ['UN', 'MIL', 'M', 'KG', 'HR'];

  protected readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    category: ['produto_principal', Validators.required],
    description: [''],
    billingMethod: ['Por unidade', Validators.required],
    defaultUnitPrice: [0, Validators.required],
    defaultProductionSector: ['Preparacao', Validators.required],
    fiscalNcm: ['8208.90.00', Validators.required],
    fiscalCfop: ['5101', Validators.required],
    fiscalCommercialUnit: ['UN', Validators.required],
    fiscalOriginCode: ['0', Validators.required],
    fiscalIcmsSituationCode: ['00', Validators.required],
    fiscalIpiSituationCode: ['99', Validators.required],
    fiscalPisSituationCode: ['49', Validators.required],
    fiscalCofinsSituationCode: ['49', Validators.required],
    fiscalIcmsRate: [18, Validators.required],
    fiscalIpiRate: [0, Validators.required],
    fiscalPisRate: [1.65, Validators.required],
    fiscalCofinsRate: [7.6, Validators.required],
    active: [true],
    materialRequirements: this.fb.array([this.createRequirementForm()])
  });

  constructor() {
    void this.load();
  }

  protected get materialRequirements(): FormArray {
    return this.form.controls.materialRequirements;
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  protected newProduct(): void {
    this.selectedProductId.set(null);
    this.form.reset({
      name: '',
      category: 'produto_principal',
      description: '',
      billingMethod: 'Por unidade',
      defaultUnitPrice: 0,
      defaultProductionSector: 'Preparacao',
      fiscalNcm: '8208.90.00',
      fiscalCfop: '5101',
      fiscalCommercialUnit: 'UN',
      fiscalOriginCode: '0',
      fiscalIcmsSituationCode: '00',
      fiscalIpiSituationCode: '99',
      fiscalPisSituationCode: '49',
      fiscalCofinsSituationCode: '49',
      fiscalIcmsRate: 18,
      fiscalIpiRate: 0,
      fiscalPisRate: 1.65,
      fiscalCofinsRate: 7.6,
      active: true,
      materialRequirements: []
    });
    this.materialRequirements.clear();
    this.materialRequirements.push(this.createRequirementForm());
    this.message.set('');
  }

  protected addRequirement(): void {
    this.materialRequirements.push(this.createRequirementForm());
  }

  protected removeRequirement(index: number): void {
    if (this.materialRequirements.length > 1) {
      this.materialRequirements.removeAt(index);
    }
  }

  protected onMaterialChange(index: number): void {
    const group = this.materialRequirements.at(index);
    const materialId = group.get('materialId')?.value as string;
    const material = this.materials().find((item) => item.id === materialId);
    group.patchValue({
      materialName: material?.name ?? '',
      unit: material?.unit ?? 'un'
    });
  }

  protected selectProduct(product: ProductTemplate): void {
    this.selectedProductId.set(product.id);
    this.materialRequirements.clear();
    for (const requirement of product.materialRequirements) {
      this.materialRequirements.push(
        this.fb.nonNullable.group({
          materialId: [requirement.materialId, Validators.required],
          materialName: [requirement.materialName],
          quantityPerUnit: [requirement.quantityPerUnit, [Validators.required, Validators.min(0.01)]],
          unit: [requirement.unit, Validators.required]
        })
      );
    }

    if (product.materialRequirements.length === 0) {
      this.materialRequirements.push(this.createRequirementForm());
    }

    this.form.patchValue({
      name: product.name,
      category: product.category,
      description: product.description,
      billingMethod: product.billingMethod,
      defaultUnitPrice: product.defaultUnitPrice,
      defaultProductionSector: product.defaultProductionSector,
      fiscalNcm: product.fiscalNcm,
      fiscalCfop: product.fiscalCfop,
      fiscalCommercialUnit: product.fiscalCommercialUnit,
      fiscalOriginCode: product.fiscalOriginCode,
      fiscalIcmsSituationCode: product.fiscalIcmsSituationCode,
      fiscalIpiSituationCode: product.fiscalIpiSituationCode,
      fiscalPisSituationCode: product.fiscalPisSituationCode,
      fiscalCofinsSituationCode: product.fiscalCofinsSituationCode,
      fiscalIcmsRate: product.fiscalIcmsRate,
      fiscalIpiRate: product.fiscalIpiRate,
      fiscalPisRate: product.fiscalPisRate,
      fiscalCofinsRate: product.fiscalCofinsRate,
      active: product.active
    });
    this.message.set(`Editando ${product.name}.`);
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
      const selectedProductId = this.selectedProductId();
      const product = selectedProductId
        ? await firstValueFrom(this.productsApi.update(selectedProductId, payload))
        : await firstValueFrom(this.productsApi.create(payload));

      const products = [...this.products().filter((item) => item.id !== product.id), product].sort((left, right) =>
        left.name.localeCompare(right.name)
      );

      this.products.set(products);
      this.selectedProductId.set(product.id);
      this.message.set(selectedProductId ? `Produto ${product.name} atualizado.` : `Produto ${product.name} criado.`);
    } finally {
      this.saving.set(false);
    }
  }

  protected categoryLabel(category: string): string {
    return scopeCategoryLabels[category] ?? category;
  }

  private createRequirementForm() {
    return this.fb.nonNullable.group({
      materialId: ['', Validators.required],
      materialName: [''],
      quantityPerUnit: [1, [Validators.required, Validators.min(0.01)]],
      unit: ['un', Validators.required]
    });
  }

  private buildPayload(): ProductTemplatePayload {
    const raw = this.form.getRawValue();
    const materialRequirements = raw.materialRequirements
      .map((requirement) => {
        const material = this.materials().find((item) => item.id === requirement.materialId);
        return {
          materialId: requirement.materialId,
          materialName: material?.name ?? requirement.materialName,
          quantityPerUnit: Number(requirement.quantityPerUnit),
          unit: material?.unit ?? requirement.unit
        };
      })
      .filter((requirement) => requirement.materialId && requirement.quantityPerUnit > 0);

    return {
      name: raw.name,
      category: raw.category,
      description: raw.description,
      billingMethod: raw.billingMethod,
      defaultUnitPrice: Number(raw.defaultUnitPrice),
      defaultProductionSector: raw.defaultProductionSector,
      fiscalNcm: raw.fiscalNcm,
      fiscalCfop: raw.fiscalCfop,
      fiscalCommercialUnit: raw.fiscalCommercialUnit,
      fiscalOriginCode: raw.fiscalOriginCode,
      fiscalIcmsSituationCode: raw.fiscalIcmsSituationCode,
      fiscalIpiSituationCode: raw.fiscalIpiSituationCode,
      fiscalPisSituationCode: raw.fiscalPisSituationCode,
      fiscalCofinsSituationCode: raw.fiscalCofinsSituationCode,
      fiscalIcmsRate: Number(raw.fiscalIcmsRate),
      fiscalIpiRate: Number(raw.fiscalIpiRate),
      fiscalPisRate: Number(raw.fiscalPisRate),
      fiscalCofinsRate: Number(raw.fiscalCofinsRate),
      active: raw.active,
      materialRequirements
    };
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const [products, materials] = await Promise.all([
        firstValueFrom(this.productsApi.list()),
        firstValueFrom(this.inventoryApi.listMaterials())
      ]);

      this.products.set(products);
      this.materials.set(materials);

      if (!this.selectedProductId()) {
        this.newProduct();
      }
    } finally {
      this.loading.set(false);
    }
  }
}
