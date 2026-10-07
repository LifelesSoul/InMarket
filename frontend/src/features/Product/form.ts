import { fail, ok, type Result } from '../../shared/auth/result';
import { ProductStatus, type NewProduct, type Product } from './types';

export const TITLE_MAX_LENGTH = 250;

export interface ProductFormValues {
  title: string;
  price: string;
  description: string;
  categoryId: string;
  imageUrls: string;
  status: ProductStatus;
}

export const EMPTY_PRODUCT_FORM: ProductFormValues = {
  title: '',
  price: '',
  description: '',
  categoryId: '',
  imageUrls: '',
  status: ProductStatus.Available,
};

const PRICE_PATTERN = /^\d{1,16}(?:\.\d{1,2})?$/;

export function toFormValues(product: Product): ProductFormValues {
  return {
    title: product.title,
    price: String(product.price),
    description: product.description ?? '',
    categoryId: product.categoryId,
    imageUrls: product.imageUrls.join('\n'),
    status: product.status,
  };
}

function isHttpLink(value: string): boolean {
  try {
    const url = new URL(value);
    return url.protocol === 'http:' || url.protocol === 'https:';
  } catch {
    return false;
  }
}

export function readProductForm(values: ProductFormValues): Result<NewProduct, string> {
  const title = values.title.trim();
  const price = values.price.trim().replace(',', '.');
  const description = values.description.trim();
  const imageUrls = values.imageUrls
    .split('\n')
    .map((line) => line.trim())
    .filter((line) => line.length > 0);

  if (title.length === 0) {
    return fail('Enter a title.');
  }

  if (title.length > TITLE_MAX_LENGTH) {
    return fail(`The title must be at most ${TITLE_MAX_LENGTH} characters.`);
  }

  if (!PRICE_PATTERN.test(price) || Number(price) <= 0) {
    return fail('Enter a price greater than 0 with at most 2 decimal places.');
  }

  if (values.categoryId === '') {
    return fail('Choose a category.');
  }

  const badLink = imageUrls.find((url) => !isHttpLink(url));

  if (badLink !== undefined) {
    return fail(`"${badLink}" is not an http or https link.`);
  }

  return ok({
    title,
    price: Number(price),
    description: description === '' ? null : description,
    categoryId: values.categoryId,
    imageUrls,
  });
}
