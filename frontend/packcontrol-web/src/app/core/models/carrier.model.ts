export interface Carrier {
  id: string;
  name: string;
  contactName: string;
  email: string;
  phone: string;
  businessHours: string;
  serviceArea: string;
  defaultMode: string;
  doesPickup: boolean;
  doesDelivery: boolean;
  notes: string;
  updatedAtUtc: string;
}

export interface CarrierPayload {
  name: string;
  contactName: string;
  email: string;
  phone: string;
  businessHours: string;
  serviceArea: string;
  defaultMode: string;
  doesPickup: boolean;
  doesDelivery: boolean;
  notes: string;
}
