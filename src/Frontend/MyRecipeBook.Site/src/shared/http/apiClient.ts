import axios, {
  AxiosError,
  type AxiosInstance,
  type AxiosRequestConfig,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from 'axios';

import { env } from '@shared/config';

export type ApiResult<TResponse> = TResponse | undefined;

export type ApiErrorDetails = {
  statusCode?: number;
  errors: string[];
  tokenIsExpired: boolean;
  isNetworkError: boolean;
};

export type AccessTokenProvider = () => string | null;
export type RefreshTokenHandler = () => Promise<string | null>;

export type ConfigureHttpAuthOptions = {
  getAccessToken?: AccessTokenProvider;
  refreshToken?: RefreshTokenHandler;
};

export type HttpRequestConfig<TBody = unknown> = AxiosRequestConfig<TBody> & {
  skipAuth?: boolean;
};

type ApiClientRequestConfig<TBody = unknown> = InternalAxiosRequestConfig<TBody> & {
  skipAuth?: boolean;
  hasRetriedAfterRefresh?: boolean;
};

export class ApiRequestError extends Error implements ApiErrorDetails {
  readonly statusCode?: number;
  readonly errors: string[];
  readonly tokenIsExpired: boolean;
  readonly isNetworkError: boolean;

  constructor(details: ApiErrorDetails) {
    super(details.errors[0] ?? 'Unexpected API error.');
    this.name = 'ApiRequestError';
    this.statusCode = details.statusCode;
    this.errors = details.errors;
    this.tokenIsExpired = details.tokenIsExpired;
    this.isNetworkError = details.isNetworkError;
  }
}

let accessTokenProvider: AccessTokenProvider = () => null;
let refreshTokenHandler: RefreshTokenHandler | null = null;

export const apiClient: AxiosInstance = axios.create({
  baseURL: env.apiVersionedBaseUrl,
  headers: {
    Accept: 'application/json',
  },
});

export function configureHttpAuth(options: ConfigureHttpAuthOptions): void {
  accessTokenProvider = options.getAccessToken ?? (() => null);
  refreshTokenHandler = options.refreshToken ?? null;
}

export function clearHttpAuth(): void {
  accessTokenProvider = () => null;
  refreshTokenHandler = null;
}

export function isApiRequestError(error: unknown): error is ApiRequestError {
  return error instanceof ApiRequestError;
}

apiClient.interceptors.request.use((config) => {
  const requestConfig = config as ApiClientRequestConfig;

  if (requestConfig.skipAuth) {
    return requestConfig;
  }

  const token = accessTokenProvider();

  if (token) {
    requestConfig.headers.set('Authorization', `Bearer ${token}`);
  }

  return requestConfig;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<unknown>) => {
    const apiError = toApiRequestError(error);
    const originalRequest = error.config as ApiClientRequestConfig | undefined;

    if (shouldTryRefreshToken(apiError, originalRequest)) {
      originalRequest.hasRetriedAfterRefresh = true;

      try {
        const refreshedAccessToken = await refreshTokenHandler?.();

        if (refreshedAccessToken) {
          originalRequest.headers.set('Authorization', `Bearer ${refreshedAccessToken}`);
          return apiClient.request(originalRequest);
        }
      } catch {
        return Promise.reject(apiError);
      }
    }

    return Promise.reject(apiError);
  },
);

async function request<TResponse = void, TBody = unknown>(
  config: HttpRequestConfig<TBody>,
): Promise<ApiResult<TResponse>> {
  const response = await apiClient.request<TResponse>(config);
  return getResponseData(response);
}

function get<TResponse = void>(
  url: string,
  config?: HttpRequestConfig,
): Promise<ApiResult<TResponse>> {
  return request<TResponse>({ ...config, method: 'GET', url });
}

function post<TResponse = void, TBody = unknown>(
  url: string,
  data?: TBody,
  config?: HttpRequestConfig<TBody>,
): Promise<ApiResult<TResponse>> {
  return request<TResponse, TBody>({ ...config, data, method: 'POST', url });
}

function put<TResponse = void, TBody = unknown>(
  url: string,
  data?: TBody,
  config?: HttpRequestConfig<TBody>,
): Promise<ApiResult<TResponse>> {
  return request<TResponse, TBody>({ ...config, data, method: 'PUT', url });
}

function patch<TResponse = void, TBody = unknown>(
  url: string,
  data?: TBody,
  config?: HttpRequestConfig<TBody>,
): Promise<ApiResult<TResponse>> {
  return request<TResponse, TBody>({ ...config, data, method: 'PATCH', url });
}

function deleteRequest<TResponse = void>(
  url: string,
  config?: HttpRequestConfig,
): Promise<ApiResult<TResponse>> {
  return request<TResponse>({ ...config, method: 'DELETE', url });
}

function getResponseData<TResponse>(response: AxiosResponse<TResponse>): ApiResult<TResponse> {
  if (response.status === 204) {
    return undefined;
  }

  return response.data;
}

function shouldTryRefreshToken(
  error: ApiRequestError,
  originalRequest: ApiClientRequestConfig | undefined,
): originalRequest is ApiClientRequestConfig {
  return (
    Boolean(originalRequest) &&
    !originalRequest?.skipAuth &&
    !originalRequest?.hasRetriedAfterRefresh &&
    error.statusCode === 401 &&
    error.tokenIsExpired &&
    refreshTokenHandler !== null
  );
}

function toApiRequestError(error: AxiosError<unknown>): ApiRequestError {
  const response = error.response;
  const parsedErrors = parseApiErrors(response?.data);
  const errors = parsedErrors.length > 0 ? parsedErrors : [getFallbackErrorMessage(error)];

  return new ApiRequestError({
    statusCode: response?.status,
    errors,
    tokenIsExpired: parseTokenIsExpired(response?.data),
    isNetworkError: response === undefined,
  });
}

function getFallbackErrorMessage(error: AxiosError<unknown>): string {
  if (error.response === undefined) {
    return 'Unable to connect to the API.';
  }

  return error.message || 'Unexpected API error.';
}

function parseApiErrors(data: unknown): string[] {
  if (!isRecord(data)) {
    return [];
  }

  const errors = readStringList(data.errors) ?? readStringList(data.Errors);

  if (errors && errors.length > 0) {
    return errors;
  }

  const message = readString(data.message) ?? readString(data.Message);
  const title = readString(data.title) ?? readString(data.Title);

  return [message, title].filter((value): value is string => Boolean(value));
}

function parseTokenIsExpired(data: unknown): boolean {
  if (!isRecord(data)) {
    return false;
  }

  return readBoolean(data.tokenIsExpired) ?? readBoolean(data.TokenIsExpired) ?? false;
}

function readStringList(value: unknown): string[] | undefined {
  if (!Array.isArray(value)) {
    return undefined;
  }

  return value.filter((item): item is string => typeof item === 'string' && item.trim().length > 0);
}

function readString(value: unknown): string | undefined {
  return typeof value === 'string' && value.trim().length > 0 ? value : undefined;
}

function readBoolean(value: unknown): boolean | undefined {
  return typeof value === 'boolean' ? value : undefined;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}

export const httpClient = {
  delete: deleteRequest,
  get,
  patch,
  post,
  put,
  request,
};
