export type ErrorKind = 'http' | 'network' | 'cancelled' | 'invalid-response';

export interface NormalizedError {
  kind: ErrorKind;
  status?: number;
  code?: string;
  traceId?: string;
  message: string;
  errors?: Record<string, string[]>;
}

export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance: string;
  code: string;
  traceId: string;
  errors?: Record<string, string[]>;
}
