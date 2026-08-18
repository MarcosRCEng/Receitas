import type { TokensResponse } from '../types';

const AUTH_TOKENS_STORAGE_KEY = 'myrecipebook.auth.tokens';

function getLocalStorage(): Storage | null {
  return typeof window === 'undefined' ? null : window.localStorage;
}

export function persistAuthTokens(tokens: TokensResponse): void {
  if (!isTokensResponse(tokens)) {
    throw new Error('Authentication tokens must include a valid access token and refresh token.');
  }

  getLocalStorage()?.setItem(AUTH_TOKENS_STORAGE_KEY, JSON.stringify(tokens));
}

export function getStoredAuthTokens(): TokensResponse | null {
  const rawTokens = getLocalStorage()?.getItem(AUTH_TOKENS_STORAGE_KEY);

  if (!rawTokens) {
    return null;
  }

  try {
    const parsedTokens: unknown = JSON.parse(rawTokens);

    if (!isTokensResponse(parsedTokens)) {
      clearStoredAuthTokens();
      return null;
    }

    return parsedTokens;
  } catch {
    clearStoredAuthTokens();
    return null;
  }
}

export function getStoredAccessToken(): string | null {
  return getStoredAuthTokens()?.accessToken ?? null;
}

export function clearStoredAuthTokens(): void {
  getLocalStorage()?.removeItem(AUTH_TOKENS_STORAGE_KEY);
}

function isTokensResponse(value: unknown): value is TokensResponse {
  return (
    typeof value === 'object' &&
    value !== null &&
    'accessToken' in value &&
    'refreshToken' in value &&
    isNonEmptyString(value.accessToken) &&
    isNonEmptyString(value.refreshToken)
  );
}

function isNonEmptyString(value: unknown): value is string {
  return typeof value === 'string' && value.trim().length > 0;
}
