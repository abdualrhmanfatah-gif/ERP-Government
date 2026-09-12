import {
  RecurringEntryDto,
  CreateRecurringEntryCommand,
  PauseRecurringEntryCommand,
  ResumeRecurringEntryCommand,
  CancelRecurringEntryCommand,
  RecurringFrequency,
  RecurringEntryStatus,
} from '../../../../web-api-client';

export type {
  RecurringEntryDto,
  CreateRecurringEntryCommand,
  PauseRecurringEntryCommand,
  ResumeRecurringEntryCommand,
  CancelRecurringEntryCommand,
  RecurringFrequency,
  RecurringEntryStatus,
};

export const FREQUENCY_LABELS: Record<string, string> = {
  Weekly: 'أسبوعي',
  Monthly: 'شهري',
  Quarterly: 'ربع سنوي',
  Yearly: 'سنوي',
};

export const STATUS_LABELS: Record<string, string> = {
  Active: 'نشط',
  Paused: 'متوقف',
  Completed: 'مكتمل',
  Cancelled: 'ملغى',
};
