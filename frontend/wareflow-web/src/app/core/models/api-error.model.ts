export interface ApiError {
  readonly status: number;
  readonly code: string;
  readonly message: string;
  readonly traceId?: string;
  readonly validationErrors?: Readonly<Record<string, readonly string[]>>;
}
