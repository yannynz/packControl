export interface RegisterEntry {
  id: string;
  groupKey: string;
  groupLabel: string;
  name: string;
  description: string;
  active: boolean;
  updatedAtUtc: string;
}

export interface RegisterGroup {
  key: string;
  label: string;
  entries: RegisterEntry[];
}

export interface RegistersOverview {
  groups: RegisterGroup[];
}
