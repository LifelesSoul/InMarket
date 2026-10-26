export const ProductStatus = {
  Draft: 1,
  Available: 2,
  Sold: 3,
  Removed: 4,
} as const;

export type ProductStatus = (typeof ProductStatus)[keyof typeof ProductStatus];

export const PRODUCT_STATUS_LABELS: Readonly<Record<ProductStatus, string>> = {
  [ProductStatus.Draft]: 'Draft',
  [ProductStatus.Available]: 'Available',
  [ProductStatus.Sold]: 'Sold',
  [ProductStatus.Removed]: 'Removed',
};

export interface Seller {
  id: string;
  username: string;
}

export interface Product {
  id: string;
  title: string;
  price: number;
  description: string | null;
  categoryId: string;
  categoryName: string;
  status: ProductStatus;
  imageUrl: string | null;
  imageUrls: string[];
  createdAt: string;
  seller: Seller;
}

export interface NewProduct {
  title: string;
  price: number;
  description: string | null;
  categoryId: string;
  imageUrls: string[];
}

export interface ProductChanges extends NewProduct {
  id: string;
  status: ProductStatus;
}

export interface PagedResult<T> {
  items: T[];
  lastId?: string | null;
}
