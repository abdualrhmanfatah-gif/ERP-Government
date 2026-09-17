import { Component, type ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(): State {
    return { hasError: true };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo): void {
    console.error('ErrorBoundary caught:', error, errorInfo);
  }

  handleRetry = (): void => {
    this.setState({ hasError: false });
    window.location.reload();
  };

  handleGoHome = (): void => {
    this.setState({ hasError: false });
    window.location.href = '/';
  };

  render(): ReactNode {
    if (this.state.hasError) {
      return (
        <div className="flex min-h-screen items-center justify-center bg-background p-4" dir="rtl">
          <div className="max-w-md rounded-lg border bg-card p-8 text-center shadow-sm">
            <h2 className="mb-2 text-lg font-semibold text-foreground">
              حدث خطأ غير متوقع
            </h2>
            <p className="mb-6 text-sm text-muted-foreground">
              حدث خطأ أثناء عرض الصفحة. يرجى المحاولة مرة أخرى.
            </p>
            <div className="flex justify-center gap-3">
              <button
                onClick={this.handleRetry}
                className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90"
              >
                إعادة المحاولة
              </button>
              <button
                onClick={this.handleGoHome}
                className="rounded-md border bg-background px-4 py-2 text-sm font-medium text-foreground hover:bg-accent"
              >
                العودة للرئيسية
              </button>
            </div>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
