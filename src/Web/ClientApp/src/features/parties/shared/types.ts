export enum PartyType {
  Supplier = 0,
  Customer = 1,
  GovEntity = 2,
  TaxAuthority = 3,
  Other = 4,
}

export const PARTY_TYPE_LABELS: Record<PartyType, string> = {
  [PartyType.Supplier]: 'مورد',
  [PartyType.Customer]: 'عميل',
  [PartyType.GovEntity]: 'جهة حكومية',
  [PartyType.TaxAuthority]: 'جهة ضريبية',
  [PartyType.Other]: 'أخرى',
};

export interface PartyResponse {
  id: number;
  partyCode: string;
  partyType: PartyType;
  nameAr: string;
  nameEn: string | null;
  taxNumber: string | null;
  nationalId: string | null;
  phone: string | null;
  email: string | null;
  address: string | null;
  notes: string | null;
  isActive: boolean;
}

export interface CreatePartyCommand {
  partyType: PartyType;
  nameAr: string;
  nameEn?: string;
  taxNumber?: string;
  nationalId?: string;
  phone?: string;
  email?: string;
  address?: string;
  notes?: string;
}

export interface UpdatePartyCommand {
  partyType: PartyType;
  nameAr: string;
  nameEn?: string;
  taxNumber?: string;
  nationalId?: string;
  phone?: string;
  email?: string;
  address?: string;
  notes?: string;
}

export interface PartyFilters {
  partyType?: PartyType;
  isActive?: boolean;
  search?: string;
}

export interface PartyDocumentResponse {
  documentType: string;
  documentId: number;
  documentNumber: string;
  status: string;
  date: string;
  amount: number;
}
