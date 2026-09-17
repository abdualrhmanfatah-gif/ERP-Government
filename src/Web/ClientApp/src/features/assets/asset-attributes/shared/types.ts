export type AssetAttributeDataType = 'Text' | 'Integer' | 'Decimal' | 'Date' | 'Boolean';

export interface AssetAttributeDefinition {
  id: number;
  code: string;
  name: string;
  description?: string;
  unit?: string;
  sortOrder: number;
  attributeDataType: AssetAttributeDataType;
  isActive: boolean;
  linkedValueCount: number;
  rowVersion: string;
}

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateAssetAttributeDefinitionRequest {
  code: string;
  name: string;
  description?: string;
  unit?: string;
  sortOrder: number;
  attributeDataType: AssetAttributeDataType;
}

export interface UpdateAssetAttributeDefinitionRequest {
  name: string;
  description?: string;
  unit?: string;
  sortOrder: number;
  attributeDataType: AssetAttributeDataType;
  isActive: boolean;
  rowVersion: string;
}

export const dataTypeLabels: Record<AssetAttributeDataType, string> = {
  Text: 'نصي',
  Integer: 'صحيح',
  Decimal: 'عشري',
  Date: 'تاريخ',
  Boolean: 'منطقي',
};

export const dataTypeOptions = Object.entries(dataTypeLabels).map(([value, label]) => ({
  value,
  label,
}));
