export type ItemType = 'Goods' | 'Service' | 'RawMaterial' | 'Consumable' | 'FixedAsset';

export interface Item {
  id: number;
  code: string;
  name: string;
  nameEn?: string;
  description?: string;
  categoryId?: number;
  categoryName?: string;
  unitId: number;
  unitName: string;
  barcode?: string;
  itemType: ItemType;
  openingStock?: number;
  availableQuantity?: number;
  reservedQuantity?: number;
  averageCost?: number;
  minimumStock?: number;
  maximumStock?: number;
  reorderLevel?: number;
  reorderQuantity?: number;
  leadTimeDays?: number;
  isActive: boolean;
}

export interface ItemDetail extends Item {
  supplierId?: number;
  itemUnits: ItemUnit[];
}

export interface ItemUnit {
  id: number;
  itemId: number;
  unitId: number;
  unitName: string;
  conversionFactor: number;
  isBase: boolean;
}

export const itemTypeLabels: Record<ItemType, string> = {
  Goods: 'بضائع',
  Service: 'خدمات',
  RawMaterial: 'مواد خام',
  Consumable: 'مستهلكات',
  FixedAsset: 'أصول ثابتة',
};

export const itemTypeOptions = Object.entries(itemTypeLabels).map(([value, label]) => ({ value, label }));
