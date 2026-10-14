import { apiRequest, describeApiError, type ApiRequest, type TokenSource } from '../../shared/api/client';
import { describeAuthError } from '../../shared/auth/errors';
import { fail, ok, type Result } from '../../shared/auth/result';
import { isFilledString, isJsonObject, isOptionalString, isStringArray } from '../../shared/json';
import {
  ProductStatus,
  type Category,
  type NewProduct,
  type PagedResult,
  type Product,
  type ProductChanges,
  type Seller,
} from './types';

export const MY_PRODUCTS_PAGE_SIZE = 10;

interface ProductResponse {
  id: string;
  title: string;
  price: number;
  description?: string | null;
  categoryId: string;
  categoryName: string;
  status: ProductStatus;
  imageUrl?: string | null;
  imageUrls: string[];
  createdAt: string;
  seller: Seller;
}

interface PageResponse {
  items: unknown[];
  lastId?: string | null;
}

const PRODUCT_STATUSES = new Set<unknown>(Object.values(ProductStatus));

function isProductStatus(value: unknown): value is ProductStatus {
  return PRODUCT_STATUSES.has(value);
}

function isSeller(value: unknown): value is Seller {
  return isJsonObject(value) && isFilledString(value.id) && typeof value.username === 'string';
}

function isProductResponse(value: unknown): value is ProductResponse {
  return isJsonObject(value)
    && isFilledString(value.id)
    && typeof value.title === 'string'
    && typeof value.price === 'number'
    && isOptionalString(value.description)
    && isFilledString(value.categoryId)
    && typeof value.categoryName === 'string'
    && isProductStatus(value.status)
    && isOptionalString(value.imageUrl)
    && isStringArray(value.imageUrls)
    && isFilledString(value.createdAt)
    && isSeller(value.seller);
}

function isCategory(value: unknown): value is Category {
  return isJsonObject(value) && isFilledString(value.id) && typeof value.name === 'string';
}

function isPageResponse(value: unknown): value is PageResponse {
  return isJsonObject(value) && Array.isArray(value.items) && isOptionalString(value.lastId);
}

function toProduct(response: ProductResponse): Product {
  return {
    id: response.id,
    title: response.title,
    price: response.price,
    description: response.description ?? null,
    categoryId: response.categoryId,
    categoryName: response.categoryName,
    status: response.status,
    imageUrl: response.imageUrl ?? null,
    imageUrls: response.imageUrls,
    createdAt: response.createdAt,
    seller: { id: response.seller.id, username: response.seller.username },
  };
}

async function send(path: string, request: ApiRequest = {}): Promise<Result<unknown, string>> {
  const response = await apiRequest(path, request);

  return response.ok ? response : fail(describeApiError(response.error));
}

async function sendSigned(
  path: string,
  getToken: TokenSource,
  request: Omit<ApiRequest, 'token'> = {},
): Promise<Result<unknown, string>> {
  const token = await getToken();

  if (!token.ok) {
    return fail(describeAuthError(token.error));
  }

  return send(path, { ...request, token: token.value });
}

function readProduct(result: Result<unknown, string>): Result<Product, string> {
  if (!result.ok) {
    return result;
  }

  if (!isProductResponse(result.value)) {
    return fail('The server returned a product in an unexpected format');
  }

  return ok(toProduct(result.value));
}

function productPath(id: string): string {
  return `/products/${encodeURIComponent(id)}`;
}

export async function fetchProduct(id: string): Promise<Result<Product, string>> {
  return readProduct(await send(productPath(id)));
}

export async function fetchCategories(): Promise<Result<Category[], string>> {
  const result = await send('/category');

  if (!result.ok) {
    return result;
  }

  const categories = result.value;

  if (!Array.isArray(categories) || !categories.every(isCategory)) {
    return fail('The server returned categories in an unexpected format');
  }

  return ok(categories);
}

export async function fetchMyProducts(
  getToken: TokenSource,
  lastId: string | null,
): Promise<Result<PagedResult<Product>, string>> {
  const query = new URLSearchParams({ limit: String(MY_PRODUCTS_PAGE_SIZE) });

  if (lastId !== null) {
    query.set('lastId', lastId);
  }

  const result = await sendSigned(`/products/mine?${query.toString()}`, getToken);

  if (!result.ok) {
    return result;
  }

  const page = result.value;

  if (!isPageResponse(page) || !page.items.every(isProductResponse)) {
    return fail('The server returned products in an unexpected format');
  }

  return ok({ items: page.items.map(toProduct), lastId: page.lastId ?? null });
}

export async function createProduct(getToken: TokenSource, product: NewProduct): Promise<Result<Product, string>> {
  return readProduct(await sendSigned('/products', getToken, { method: 'POST', body: product }));
}

export async function updateProduct(getToken: TokenSource, changes: ProductChanges): Promise<Result<Product, string>> {
  return readProduct(await sendSigned('/products', getToken, { method: 'PUT', body: changes }));
}

export async function deleteProduct(getToken: TokenSource, id: string): Promise<Result<void, string>> {
  const result = await sendSigned(productPath(id), getToken, { method: 'DELETE' });

  return result.ok ? ok(undefined) : result;
}
