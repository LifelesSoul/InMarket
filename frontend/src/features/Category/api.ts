import { apiCall, apiCallWithToken, type TokenSource } from '../../shared/api/client';
import { fail, ok, type Result } from '../../shared/auth/result';
import { isFilledString, isJsonObject } from '../../shared/json';
import type { Category } from './types';

function isCategory(value: unknown): value is Category {
  return isJsonObject(value) && isFilledString(value.id) && typeof value.name === 'string';
}

function readCategory(result: Result<unknown, string>): Result<Category, string> {
  if (!result.ok) {
    return result;
  }

  if (!isCategory(result.value)) {
    return fail('The server returned a category in an unexpected format');
  }

  return ok({ id: result.value.id, name: result.value.name });
}

export async function fetchCategories(): Promise<Result<Category[], string>> {
  const result = await apiCall('/category');

  if (!result.ok) {
    return result;
  }

  const categories = result.value;

  if (!Array.isArray(categories) || !categories.every(isCategory)) {
    return fail('The server returned categories in an unexpected format');
  }

  return ok(categories.map((category) => ({ id: category.id, name: category.name })));
}

export async function createCategory(getToken: TokenSource, name: string): Promise<Result<Category, string>> {
  return readCategory(await apiCallWithToken('/category', getToken, { method: 'POST', body: { name } }));
}

export async function renameCategory(getToken: TokenSource, category: Category): Promise<Result<Category, string>> {
  return readCategory(await apiCallWithToken('/category', getToken, { method: 'PUT', body: category }));
}

export async function deleteCategory(getToken: TokenSource, id: string): Promise<Result<void, string>> {
  const result = await apiCallWithToken(`/category/${encodeURIComponent(id)}`, getToken, { method: 'DELETE' });

  return result.ok ? ok(undefined) : result;
}
