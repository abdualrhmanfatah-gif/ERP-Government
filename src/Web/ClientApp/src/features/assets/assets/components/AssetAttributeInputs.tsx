import { useMemo } from 'react';
import type { AssetGroupAttributeBinding } from '../../asset-groups/shared/types';
import type { AssetAttributeValue, AssetAttributeValueInput } from '../shared/types';
import { EmptyState } from '@/components/ui/EmptyState';
import { Input } from '@/components/ui/Input';
import { Switch } from '@/components/ui/Switch';

export interface AssetAttributeInputValue {
  textValue?: string;
  integerValue?: string;
  decimalValue?: string;
  dateValue?: string;
  booleanValue?: boolean;
}

export type AssetAttributeInputState = Record<number, AssetAttributeInputValue>;

export function sortAttributeBindings(bindings: AssetGroupAttributeBinding[]) {
  return [...bindings].sort((a, b) => {
    const orderA = a.sortOrder ?? Number.MAX_SAFE_INTEGER;
    const orderB = b.sortOrder ?? Number.MAX_SAFE_INTEGER;
    if (orderA !== orderB) return orderA - orderB;
    return a.name.localeCompare(b.name, 'ar');
  });
}

function hasValue(binding: AssetGroupAttributeBinding, value?: AssetAttributeInputValue) {
  if (!value) return false;
  switch (binding.dataType) {
    case 'Text':
      return !!value.textValue?.trim();
    case 'Integer':
      return value.integerValue !== undefined && value.integerValue !== '';
    case 'Decimal':
      return value.decimalValue !== undefined && value.decimalValue !== '';
    case 'Date':
      return !!value.dateValue;
    case 'Boolean':
      return value.booleanValue !== undefined;
  }

  return false;
}

export function findMissingRequiredAttributes(
  bindings: AssetGroupAttributeBinding[],
  state: AssetAttributeInputState
) {
  return bindings
    .filter((binding) => binding.isRequired && !hasValue(binding, state[binding.assetAttributeDefinitionId]))
    .map((binding) => binding.name);
}

export function buildAttributeValues(
  bindings: AssetGroupAttributeBinding[],
  state: AssetAttributeInputState
): AssetAttributeValueInput[] {
  return bindings.map((binding) => {
    const value = state[binding.assetAttributeDefinitionId];
    const input: AssetAttributeValueInput = { assetAttributeDefinitionId: binding.assetAttributeDefinitionId };
    if (!value) return input;

    switch (binding.dataType) {
      case 'Text':
        if (value.textValue?.trim()) input.textValue = value.textValue.trim();
        break;
      case 'Integer':
        if (value.integerValue !== undefined && value.integerValue !== '') input.integerValue = Number(value.integerValue);
        break;
      case 'Decimal':
        if (value.decimalValue !== undefined && value.decimalValue !== '') input.decimalValue = Number(value.decimalValue);
        break;
      case 'Date':
        if (value.dateValue) input.dateValue = value.dateValue;
        break;
      case 'Boolean':
        if (value.booleanValue !== undefined) input.booleanValue = value.booleanValue;
        break;
    }

    return input;
  });
}

export function stateFromExistingValues(
  bindings: AssetGroupAttributeBinding[],
  existing: AssetAttributeValue[]
): AssetAttributeInputState {
  const byDefinition = new Map(existing.map((value) => [value.assetAttributeDefinitionId, value]));
  const state: AssetAttributeInputState = {};

  for (const binding of bindings) {
    const value = byDefinition.get(binding.assetAttributeDefinitionId);
    if (!value) continue;
    state[binding.assetAttributeDefinitionId] = {
      textValue: value.textValue,
      integerValue: value.integerValue != null ? String(value.integerValue) : undefined,
      decimalValue: value.decimalValue != null ? String(value.decimalValue) : undefined,
      dateValue: value.dateValue,
      booleanValue: value.booleanValue,
    };
  }

  return state;
}

interface AssetAttributeInputsProps {
  bindings: AssetGroupAttributeBinding[];
  values: AssetAttributeInputState;
  onChange: (definitionId: number, value: AssetAttributeInputValue) => void;
  disabled?: boolean;
}

export function AssetAttributeInputs({ bindings, values, onChange, disabled }: AssetAttributeInputsProps) {
  const sorted = useMemo(() => sortAttributeBindings(bindings), [bindings]);

  if (sorted.length === 0) {
    return <EmptyState message="لا توجد مواصفات مرتبطة بهذه المجموعة" />;
  }

  return (
    <div className="flex flex-wrap items-end gap-3">
      {sorted.map((binding) => {
        const value = values[binding.assetAttributeDefinitionId] ?? {};
        const setValue = (patch: AssetAttributeInputValue) =>
          onChange(binding.assetAttributeDefinitionId, { ...value, ...patch });

        if (binding.dataType === 'Boolean') {
          return (
            <div key={binding.assetAttributeDefinitionId} className="min-w-44 flex-1">
              <span className="text-label-md text-[var(--color-on-surface)]">
                {binding.name}
                {binding.isRequired && (
                  <span aria-hidden="true" className="text-[var(--color-error)] ms-1">*</span>
                )}
              </span>
              <div className="flex min-h-[var(--density-comfortable-control-height)] items-center">
                <Switch
                  checked={value.booleanValue ?? false}
                  onChange={(checked) => setValue({ booleanValue: checked })}
                  disabled={disabled}
                  label={value.booleanValue ? 'نعم' : 'لا'}
                />
              </div>
            </div>
          );
        }

        const inputProps =
          binding.dataType === 'Integer'
            ? { type: 'number' as const, step: '1' }
            : binding.dataType === 'Decimal'
              ? { type: 'number' as const, step: '0.01' }
              : binding.dataType === 'Date'
                ? { type: 'date' as const }
                : { type: 'text' as const };

        const currentValue =
          binding.dataType === 'Integer'
            ? (value.integerValue ?? '')
            : binding.dataType === 'Decimal'
              ? (value.decimalValue ?? '')
              : binding.dataType === 'Date'
                ? (value.dateValue ?? '')
                : (value.textValue ?? '');

        return (
          <div key={binding.assetAttributeDefinitionId} className="min-w-48 flex-1">
            <Input
              {...inputProps}
              label={binding.name}
              required={binding.isRequired}
              disabled={disabled}
              dir={binding.dataType === 'Text' ? undefined : 'ltr'}
              className={binding.dataType === 'Text' ? undefined : 'tabular-nums'}
              value={currentValue}
              onChange={(event) => {
                const next = event.target.value;
                if (binding.dataType === 'Integer') setValue({ integerValue: next });
                else if (binding.dataType === 'Decimal') setValue({ decimalValue: next });
                else if (binding.dataType === 'Date') setValue({ dateValue: next });
                else setValue({ textValue: next });
              }}
            />
          </div>
        );
      })}
    </div>
  );
}
