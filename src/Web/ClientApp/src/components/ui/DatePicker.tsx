import { Input } from './Input';

interface DatePickerProps {
  label: string;
  value?: string;
  onChange: (value: string) => void;
  error?: string;
  disabled?: boolean;
  required?: boolean;
}

export function DatePicker({
  label,
  value,
  onChange,
  error,
  disabled,
  required,
}: DatePickerProps) {
  return (
    <div className="relative">
      <Input
        label={label}
        type="date"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        error={error}
        disabled={disabled}
        required={required}
      />
    </div>
  );
}
