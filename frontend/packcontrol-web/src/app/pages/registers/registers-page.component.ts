import { CommonModule, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { RegistersApiService } from '../../core/api/registers-api.service';
import { RegisterEntry, RegistersOverview } from '../../core/models/registers.model';

@Component({
  selector: 'app-registers-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DatePipe],
  templateUrl: './registers-page.component.html',
  styleUrl: './registers-page.component.scss'
})
export class RegistersPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly registersApi = inject(RegistersApiService);

  protected readonly overview = signal<RegistersOverview | null>(null);
  protected readonly selectedEntry = signal<RegisterEntry | null>(null);
  protected readonly loading = signal(true);
  protected readonly creating = signal(false);
  protected readonly saving = signal(false);
  protected readonly message = signal('');
  protected readonly totalEntries = computed(
    () => this.overview()?.groups.reduce((sum, group) => sum + group.entries.length, 0) ?? 0
  );
  protected readonly totalActiveEntries = computed(
    () =>
      this.overview()?.groups.reduce(
        (sum, group) => sum + group.entries.filter((entry) => entry.active).length,
        0
      ) ?? 0
  );

  protected readonly createForm = this.fb.nonNullable.group({
    groupKey: ['tipos_faca', Validators.required],
    name: ['', Validators.required],
    description: ['']
  });

  protected readonly editForm = this.fb.nonNullable.group({
    id: [''],
    groupLabel: [''],
    name: ['', Validators.required],
    description: [''],
    active: [true]
  });

  constructor() {
    void this.load();
  }

  protected async reload(): Promise<void> {
    await this.load();
  }

  protected async create(): Promise<void> {
    if (this.createForm.invalid || this.creating()) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.creating.set(true);
    this.message.set('');

    try {
      const currentGroupKey = this.createForm.controls.groupKey.value;
      const overview = await firstValueFrom(this.registersApi.create(this.createForm.getRawValue()));
      this.applyOverview(overview);
      this.createForm.reset({
        groupKey: currentGroupKey || 'tipos_faca',
        name: '',
        description: ''
      });
      this.message.set('Cadastro incluido na base.');
    } finally {
      this.creating.set(false);
    }
  }

  protected selectEntry(entry: RegisterEntry): void {
    this.selectedEntry.set(entry);
    this.editForm.reset({
      id: entry.id,
      groupLabel: entry.groupLabel,
      name: entry.name,
      description: entry.description,
      active: entry.active
    });
  }

  protected async saveSelected(): Promise<void> {
    const selectedEntry = this.selectedEntry();
    if (!selectedEntry || this.editForm.invalid || this.saving()) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.message.set('');

    try {
      const overview = await firstValueFrom(
        this.registersApi.update(selectedEntry.id, {
          name: this.editForm.controls.name.value,
          description: this.editForm.controls.description.value,
          active: this.editForm.controls.active.value
        })
      );
      this.applyOverview(overview, selectedEntry.id);
      this.message.set('Cadastro atualizado.');
    } finally {
      this.saving.set(false);
    }
  }

  private applyOverview(overview: RegistersOverview, selectedEntryId?: string): void {
    this.overview.set(overview);
    const nextSelectedEntryId = selectedEntryId ?? this.selectedEntry()?.id;
    const nextSelectedEntry = nextSelectedEntryId
      ? overview.groups.flatMap((group) => group.entries).find((entry) => entry.id === nextSelectedEntryId) ?? null
      : overview.groups.flatMap((group) => group.entries)[0] ?? null;

    if (nextSelectedEntry) {
      this.selectEntry(nextSelectedEntry);
      return;
    }

    this.selectedEntry.set(null);
    this.editForm.reset({
      id: '',
      groupLabel: '',
      name: '',
      description: '',
      active: true
    });
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    try {
      const overview = await firstValueFrom(this.registersApi.getOverview());
      this.applyOverview(overview);
    } finally {
      this.loading.set(false);
    }
  }
}
