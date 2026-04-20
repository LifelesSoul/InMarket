export interface Product {
  id: string;
  title: string;
  price: number;
  description: string | null;
  categoryName: string;
}

export interface PagedResult<T> {
  items: T[];
  lastId?: string | null;
}
