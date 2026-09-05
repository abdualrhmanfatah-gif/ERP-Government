import '@testing-library/jest-dom/vitest';
import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { ApprovalHistoryPanel } from '../components/ApprovalHistoryPanel';
import { ApprovalDecisionDto } from '../../../web-api-client';

describe('ApprovalHistoryPanel', () => {
  it('renders the empty state when there is no history', () => {
    render(<ApprovalHistoryPanel decisions={[]} />);
    expect(screen.getByText('لا يوجد سجل اعتمادات بعد')).toBeInTheDocument();
  });

  it('renders each decision with approver and reason', () => {
    render(
      <ApprovalHistoryPanel
        decisions={[
          new ApprovalDecisionDto({
            decision: 'Approved',
            decisionAt: new Date('2026-05-01T10:00:00Z'),
            approverUserId: 7,
            requiredRole: 'BUD_MGR',
            reason: 'Looks good',
          }),
        ]}
      />,
    );
    expect(screen.getByText('Approved')).toBeInTheDocument();
    expect(screen.getByText(/Looks good/)).toBeInTheDocument();
    expect(screen.getByText(/#7/)).toBeInTheDocument();
  });

  it('skips null entries', () => {
    render(
      <ApprovalHistoryPanel
        decisions={[
          null,
          new ApprovalDecisionDto({
            decision: 'Submitted',
            decisionAt: new Date('2026-05-01T09:00:00Z'),
            approverUserId: 3,
          }),
        ]}
      />,
    );
    expect(screen.getByText('Submitted')).toBeInTheDocument();
    expect(screen.queryByText('لا يوجد سجل اعتمادات بعد')).not.toBeInTheDocument();
  });
});
