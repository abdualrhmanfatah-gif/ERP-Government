export interface DocumentSequenceDto {
  id: number;
  name: string;
  documentType: string;
  fiscalYearId?: number | null;
  currentNumber: number;
  resetPolicy: 'Yearly' | 'Never';
  isActive: boolean;
  createdAt?: string;
  createdById?: string;
  updatedAt?: string;
  updatedById?: string;
}

export interface CreateDocumentSequenceCommand {
  name: string;
  documentType: string;
  fiscalYearId?: number | null;
  resetPolicy: 'Yearly' | 'Never';
}

export interface UpdateDocumentSequenceCommand {
  id: number;
  name?: string;
  fiscalYearId?: number | null;
  resetPolicy?: 'Yearly' | 'Never';
}
