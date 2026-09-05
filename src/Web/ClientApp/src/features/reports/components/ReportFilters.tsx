import { useState, useEffect, useCallback } from 'react';
import { Button } from '@/components/ui/Button';
import { FilterBar, FilterDate, FilterSearch } from '@/components/ui';
import { Select } from '@/components/ui/Select';

export type FilterFieldType = 'date' | 'search' | 'select';

export interface FilterField {
  type: FilterFieldType;
  key: string;
  label: string;
  placeholder?: string;
  options?: { value: string; label: string }[];
  dependsOn?: string;
  fetchUrl?: string;
}

interface ReportFiltersProps {
  fields: FilterField[];
  onApply: (filters: Record<string, string | number | undefined>) => void;
}

export function ReportFilters({ fields, onApply }: ReportFiltersProps) {
  const [values, setValues] = useState<Record<string, string>>({});
  const [selectOptions, setSelectOptions] = useState<Record<string, { value: string; label: string }[]>>({});

  // Fetch options for select fields with fetchUrl
  useEffect(() => {
    fields.forEach(field => {
      if (field.type === 'select' && field.fetchUrl && !field.dependsOn) {
        fetch(field.fetchUrl)
          .then(res => res.json())
          .then(data => {
            const options = data.map((item: { id: number; name: string }) => ({
              value: String(item.id),
              label: item.name,
            }));
            setSelectOptions(prev => ({ ...prev, [field.key]: options }));
          })
          .catch(() => setSelectOptions(prev => ({ ...prev, [field.key]: [] })));
      }
    });
  }, [fields]);

  // Fetch dependent select options
  useEffect(() => {
    fields.forEach(field => {
      if (field.type === 'select' && field.fetchUrl && field.dependsOn) {
        const parentValue = values[field.dependsOn];
        if (parentValue) {
          fetch(`${field.fetchUrl}?fiscalYearId=${parentValue}`)
            .then(res => res.json())
            .then(data => {
              const options = data.map((item: { id: number; name: string }) => ({
                value: String(item.id),
                label: item.name,
              }));
              setSelectOptions(prev => ({ ...prev, [field.key]: options }));
            })
            .catch(() => setSelectOptions(prev => ({ ...prev, [field.key]: [] })));
        } else {
          setSelectOptions(prev => ({ ...prev, [field.key]: [] }));
          setValues(prev => {
            const next = { ...prev };
            delete next[field.key];
            return next;
          });
        }
      }
    });
  }, [values, fields]);

  const handleChange = useCallback((key: string, value: string) => {
    setValues(prev => ({ ...prev, [key]: value }));
  }, []);

  const handleApply = () => {
    const filters: Record<string, string | number | undefined> = {};
    fields.forEach(field => {
      const val = values[field.key];
      if (val) {
        filters[field.key] = field.type === 'select' ? Number(val) : val;
      }
    });
    onApply(filters);
  };

  const handleClear = () => {
    setValues({});
    onApply({});
  };

  const hasFilters = Object.values(values).some(v => v !== '');

  return (
    <FilterBar hasFilters={hasFilters} onClear={handleClear}>
      {fields.map(field => {
        if (field.type === 'date') {
          return (
            <FilterDate
              key={field.key}
              label={field.label}
              value={values[field.key] || ''}
              onChange={(v) => handleChange(field.key, v)}
            />
          );
        }

        if (field.type === 'search') {
          return (
            <FilterSearch
              key={field.key}
              label={field.label}
              value={values[field.key] || ''}
              onChange={(v) => handleChange(field.key, v)}
              placeholder={field.placeholder}
            />
          );
        }

        if (field.type === 'select') {
          const options = selectOptions[field.key] || field.options || [];
          const isDisabled = field.dependsOn && !values[field.dependsOn];

          return (
            <Select
              key={field.key}
              label={field.label}
              value={values[field.key] || ''}
              onChange={(e) => handleChange(field.key, e.target.value)}
              options={options}
              placeholder={field.placeholder}
              disabled={isDisabled}
            />
          );
        }

        return null;
      })}

      <Button variant="primary" size="xs" onClick={handleApply} disabled={!hasFilters}>
        عرض
      </Button>
    </FilterBar>
  );
}
