export interface RuntimeConfig {
  readonly apiBaseUrl: string;
  readonly environment: 'development' | 'production';
}

declare global {
  // Deployment may define this before Angular starts. It must contain public
  // configuration only; authentication remains cookie-based.
  var __WAREFLOW_CONFIG__: Partial<RuntimeConfig> | undefined;
}

export const runtimeConfig: RuntimeConfig = {
  apiBaseUrl: globalThis.__WAREFLOW_CONFIG__?.apiBaseUrl ?? '',
  environment: globalThis.__WAREFLOW_CONFIG__?.environment ?? 'development',
};
