export interface AssetGroup {
  id: number;
  code: string;
  name: string;
  description?: string;
  parentAssetGroupId?: number;
  parentName?: string;
  isActive: boolean;
  assetCategory: string;
  isDepreciable: boolean;
  defaultUsefulLifeYears?: number;
  hasChildren: boolean;
  rowVersion: string;
}

export interface AssetGroupDetail extends AssetGroup {
  depreciationMethod: string;
  depreciationRate?: number;
  residualValuePercentage?: number;
  assetAccountId?: number;
  accumulatedDepreciationAccountId?: number;
  depreciationExpenseAccountId?: number;
  disposalAccountId?: number;
  rowVersion: string;
  created: string;
  createdBy?: string;
  lastModified: string;
  lastModifiedBy?: string;
  attributeBindings: AssetGroupAttributeBinding[];
}

export interface AssetGroupAttributeBinding {
  assetAttributeDefinitionId: number;
  code: string;
  name: string;
  dataType: 'Text' | 'Integer' | 'Decimal' | 'Date' | 'Boolean';
  isRequired: boolean;
  sortOrder: number | null;
}

export interface CreateAssetGroupRequest {
  code: string;
  name: string;
  description?: string;
  parentAssetGroupId?: number;
  assetCategory: string;
  isDepreciable: boolean;
  depreciationMethod: string;
  depreciationRate?: number;
  defaultUsefulLifeYears?: number;
  residualValuePercentage?: number;
  assetAccountId?: number;
  accumulatedDepreciationAccountId?: number;
  depreciationExpenseAccountId?: number;
  disposalAccountId?: number;
}

export interface UpdateAssetGroupRequest {
  name: string;
  description?: string;
  parentAssetGroupId?: number;
  assetCategory: string;
  isDepreciable: boolean;
  depreciationMethod: string;
  depreciationRate?: number;
  defaultUsefulLifeYears?: number;
  residualValuePercentage?: number;
  assetAccountId?: number;
  accumulatedDepreciationAccountId?: number;
  depreciationExpenseAccountId?: number;
  disposalAccountId?: number;
  rowVersion: string;
}

export const assetCategoryLabels: Record<string, string> = {
  Tangible: 'مادية',
  Intangible: 'غير مادية',
};

export const assetCategoryOptions = Object.entries(assetCategoryLabels).map(([value, label]) => ({ value, label }));
