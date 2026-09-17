export type AssetStatus = 'Draft' | 'Active' | 'UnderMaintenance' | 'Disposed' | 'WrittenOff';

export type AcquisitionType = 'Purchase' | 'Grant' | 'Transfer' | 'Donation' | 'Inherited';

export interface Asset {
  id: number;
  code: string;
  name: string;
  assetGroupId: number;
  assetGroupName?: string;
  status: string;
  locationName?: string;
  employeeName?: string;
  currencyCode?: string;
  exchangeRate?: number;
  originalValue: number;
  currentValue?: number;
  purchaseDate: string;
  assetTag?: string;
  isActive: boolean;
  rowVersion: string;
}

export interface AssetAttributeValue {
  assetAttributeDefinitionId: number;
  code: string;
  name: string;
  dataType: 'Text' | 'Integer' | 'Decimal' | 'Date' | 'Boolean';
  isActive: boolean;
  textValue?: string;
  integerValue?: number;
  decimalValue?: number;
  dateValue?: string;
  booleanValue?: boolean;
}

export interface AssetAttributeValueInput {
  assetAttributeDefinitionId: number;
  textValue?: string;
  integerValue?: number;
  decimalValue?: number;
  dateValue?: string;
  booleanValue?: boolean;
}

export interface AssetDetail extends Asset {
  description?: string;
  locationId?: number;
  employeeId?: number;
  employeeName?: string;
  currentDepartmentId?: number;
  currentDepartment?: string;
  barcode?: string;
  serialNumber?: string;
  currencyId: number;
  currencyCode?: string;
  exchangeRateId?: number;
  exchangeRate?: number;
  acquisitionCost?: number;
  accumulatedDepreciation: number;
  activationDate?: string;
  depreciationStartDate: string;
  lastDepreciationDate?: string;
  acquisitionType: string;
  usefulLifeYears?: number;
  isFullyDepreciated: boolean;
  notes?: string;
  created: string;
  createdBy?: string;
  lastModified: string;
  lastModifiedBy?: string;
  attributeValues: AssetAttributeValue[];
}

export const assetStatusLabels: Record<AssetStatus, string> = {
  Draft: 'مسودة',
  Active: 'نشط',
  UnderMaintenance: 'تحت الصيانة',
  Disposed: 'متخلص',
  WrittenOff: 'مُشطوب',
};

export const acquisitionTypeLabels: Record<AcquisitionType, string> = {
  Purchase: 'شراء',
  Grant: 'منحة',
  Transfer: 'تحويل',
  Donation: 'هدية',
  Inherited: 'موروث',
};

export const assetStatusOptions = Object.entries(assetStatusLabels).map(([value, label]) => ({ value, label }));
export const acquisitionTypeOptions = Object.entries(acquisitionTypeLabels).map(([value, label]) => ({ value, label }));
