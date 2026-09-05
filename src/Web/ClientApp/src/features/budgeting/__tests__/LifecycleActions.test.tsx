import '@testing-library/jest-dom/vitest';
import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { LifecycleActions } from '../components/LifecycleActions';
import { actionLabel } from '../utils/budgeting-utils';

describe('actionLabel', () => {
  it('maps known lifecycle keys to Arabic labels', () => {
    expect(actionLabel('submit')).toBe('اعتماد للإرسال');
    expect(actionLabel('approve')).toBe('اعتماد');
    expect(actionLabel('reverse')).toBe('عكس');
  });

  it('falls back to the key itself', () => {
    expect(actionLabel('unknown-key')).toBe('unknown-key');
  });
});

describe('LifecycleActions', () => {
  const actions = [
    { key: 'submit', label: 'اعتماد للإرسال', permission: 'Appropriations.Submit' },
    { key: 'approve', label: 'اعتماد', permission: 'Appropriations.Approve' },
  ];

  it('renders only actions the user is permitted for', () => {
    render(
      <LifecycleActions
        actions={actions}
        can={(p) => p === 'Appropriations.Submit'}
        onAction={() => {}}
      />,
    );
    expect(screen.getByText('اعتماد للإرسال')).toBeInTheDocument();
    expect(screen.queryByText('اعتماد')).not.toBeInTheDocument();
  });

  it('invokes onAction with the action key', () => {
    const onAction = vi.fn();
    render(<LifecycleActions actions={actions} can={() => true} onAction={onAction} />);
    fireEvent.click(screen.getByText('اعتماد'));
    expect(onAction).toHaveBeenCalledWith('approve');
  });

  it('disables the pending action', () => {
    render(
      <LifecycleActions actions={actions} can={() => true} pendingKey="submit" onAction={() => {}} />,
    );
    expect(screen.getByText('اعتماد للإرسال').closest('button')).toBeDisabled();
  });

  it('renders nothing when no action is permitted', () => {
    const { container } = render(
      <LifecycleActions actions={actions} can={() => false} onAction={() => {}} />,
    );
    expect(container).toBeEmptyDOMElement();
  });
});
