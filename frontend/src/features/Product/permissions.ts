import type { MyProfile } from '../Profile/types';
import type { Product } from './types';

export function isProductOwner(product: Product, profile: MyProfile): boolean {
  return product.seller.id === profile.id;
}

export function canManageProduct(product: Product, profile: MyProfile): boolean {
  return isProductOwner(product, profile) || profile.roles.includes('Admin');
}
