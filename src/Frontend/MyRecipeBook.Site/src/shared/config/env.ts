type ViteEnvKey =
  | 'VITE_APP_NAME'
  | 'VITE_API_BASE_URL'
  | 'VITE_API_VERSION'
  | 'VITE_GOOGLE_LOGIN_ENABLED'
  | 'VITE_GOOGLE_RETURN_URL'
  | 'VITE_AI_RECIPE_GENERATION_ENABLED'
  | 'VITE_RECIPE_IMAGE_UPLOAD_ENABLED';

const defaults: Record<ViteEnvKey, string> = {
  VITE_APP_NAME: 'MyRecipeBook',
  VITE_API_BASE_URL: 'http://localhost:5113',
  VITE_API_VERSION: 'v1',
  VITE_GOOGLE_LOGIN_ENABLED: 'false',
  VITE_GOOGLE_RETURN_URL: 'http://localhost:5173/auth/google/callback',
  VITE_AI_RECIPE_GENERATION_ENABLED: 'false',
  VITE_RECIPE_IMAGE_UPLOAD_ENABLED: 'true',
};

function readEnv(key: ViteEnvKey): string {
  const value = import.meta.env[key];
  return value?.trim() ? value : defaults[key];
}

function readUrl(key: ViteEnvKey): string {
  const value = readEnv(key);

  try {
    return new URL(value).toString().replace(/\/$/, '');
  } catch {
    throw new Error(`Invalid environment variable ${key}: expected a public URL.`);
  }
}

function readBoolean(key: ViteEnvKey): boolean {
  const value = readEnv(key).toLowerCase();

  if (value === 'true') {
    return true;
  }

  if (value === 'false') {
    return false;
  }

  throw new Error(`Invalid environment variable ${key}: expected true or false.`);
}

function readApiVersion(): string {
  const value = readEnv('VITE_API_VERSION');

  if (!/^v\d+$/i.test(value)) {
    throw new Error('Invalid environment variable VITE_API_VERSION: expected format like v1.');
  }

  return value;
}

function buildApiVersionedBaseUrl(apiBaseUrl: string, apiVersion: string): string {
  return `${apiBaseUrl}/api/${apiVersion}`;
}

const apiBaseUrl = readUrl('VITE_API_BASE_URL');
const apiVersion = readApiVersion();

export const env = {
  appName: readEnv('VITE_APP_NAME'),
  apiBaseUrl,
  apiVersion,
  apiVersionedBaseUrl: buildApiVersionedBaseUrl(apiBaseUrl, apiVersion),
  googleLoginEnabled: readBoolean('VITE_GOOGLE_LOGIN_ENABLED'),
  googleReturnUrl: readUrl('VITE_GOOGLE_RETURN_URL'),
  aiRecipeGenerationEnabled: readBoolean('VITE_AI_RECIPE_GENERATION_ENABLED'),
  recipeImageUploadEnabled: readBoolean('VITE_RECIPE_IMAGE_UPLOAD_ENABLED'),
} as const;

export const environment = env;
