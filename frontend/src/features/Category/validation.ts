import { fail, ok, type Result } from '../../shared/auth/result';
import type { Category } from './types';

export const CATEGORY_NAME_MIN = 2;
export const CATEGORY_NAME_MAX = 50;

function normalize(name: string): string {
  return name.trim().replace(/\s+/g, ' ').toLowerCase();
}

export function readCategoryName(
  value: string,
  existing: readonly Category[],
  ownId?: string,
): Result<string, string> {
  const name = value.trim().replace(/\s+/g, ' ');

  if (name.length < CATEGORY_NAME_MIN || name.length > CATEGORY_NAME_MAX) {
    return fail(`The name must be ${CATEGORY_NAME_MIN} to ${CATEGORY_NAME_MAX} characters long.`);
  }

  const taken = existing.some((category) => category.id !== ownId && normalize(category.name) === normalize(name));

  if (taken) {
    return fail(`Category "${name}" already exists.`);
  }

  return ok(name);
}
