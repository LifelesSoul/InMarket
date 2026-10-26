import { describeAuthError, errorMessage, type AuthError } from '../auth/errors';
import { fail, ok, type Result } from '../auth/result';
import { isJsonObject } from '../json';

export type TokenSource = () => Promise<Result<string, AuthError>>;

export type ApiError =
  | { kind: 'network'; message: string }
  | { kind: 'http'; status: number; title: string; detail?: string }
  | { kind: 'invalid-json'; status: number };

export interface ApiRequest {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
  token?: string;
  body?: unknown;
}

const API_URL = import.meta.env.VITE_API_URL;

async function readProblem(response: Response): Promise<ApiError> {
  const text = await response.text().catch(() => '');
  const plain: ApiError = { kind: 'http', status: response.status, title: response.statusText, detail: text || undefined };
  let body: unknown;

  try {
    body = JSON.parse(text);
  } catch {
    return plain;
  }

  if (!isJsonObject(body)) {
    return plain;
  }

  return {
    kind: 'http',
    status: response.status,
    title: typeof body.title === 'string' ? body.title : response.statusText,
    detail: typeof body.detail === 'string' ? body.detail : undefined,
  };
}

export async function apiRequest(path: string, request: ApiRequest = {}): Promise<Result<unknown, ApiError>> {
  const headers = new Headers();

  if (request.token !== undefined) {
    headers.set('Authorization', `Bearer ${request.token}`);
  }

  if (request.body !== undefined) {
    headers.set('Content-Type', 'application/json');
  }

  let response: Response;

  try {
    response = await fetch(`${API_URL}${path}`, {
      method: request.method ?? 'GET',
      headers,
      body: request.body === undefined ? undefined : JSON.stringify(request.body),
    });
  } catch (err: unknown) {
    return fail({ kind: 'network', message: errorMessage(err) });
  }

  if (!response.ok) {
    return fail(await readProblem(response));
  }

  if (response.status === 204) {
    return ok(undefined);
  }

  try {
    return ok(await response.json());
  } catch {
    return fail({ kind: 'invalid-json', status: response.status });
  }
}

export function describeApiError(error: ApiError): string {
  switch (error.kind) {
    case 'network':
      return `The server could not be reached (${error.message})`;
    case 'http':
      return error.detail ?? `${error.title} (${error.status})`;
    case 'invalid-json':
      return `The server answered ${error.status} with a response that is not JSON`;
  }
}

export async function apiCall(path: string, request: ApiRequest = {}): Promise<Result<unknown, string>> {
  const response = await apiRequest(path, request);

  return response.ok ? response : fail(describeApiError(response.error));
}

export async function apiCallWithToken(
  path: string,
  getToken: TokenSource,
  request: Omit<ApiRequest, 'token'> = {},
): Promise<Result<unknown, string>> {
  const token = await getToken();

  if (!token.ok) {
    return fail(describeAuthError(token.error));
  }

  return apiCall(path, { ...request, token: token.value });
}
