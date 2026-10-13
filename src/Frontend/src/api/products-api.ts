import { apiFetch, readApiError } from './http-client';
import type { Product, ProductPage } from '../types/product';

export interface ProductFilters { minPrice?: number; maxPrice?: number; sortBy?: 'price' | 'name'; sortDirection?: 'asc' | 'desc'; }

export async function getProducts(page = 1, pageSize = 24, search = '', category?: string, filters: ProductFilters = {}): Promise<ProductPage> {
  const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  if (search.trim()) query.set('search', search.trim());
  if (category) query.set('category', category);
  if (filters.minPrice !== undefined) query.set('minPrice', String(filters.minPrice));
  if (filters.maxPrice !== undefined) query.set('maxPrice', String(filters.maxPrice));
  if (filters.sortBy) query.set('sortBy', filters.sortBy);
  if (filters.sortDirection) query.set('sortDirection', filters.sortDirection);
  const response = await apiFetch(`/api/products?${query}`);
  if (!response.ok) throw await readApiError(response, 'Не вдалося завантажити каталог.');
  return await response.json() as ProductPage;
}

export async function getProduct(id: string): Promise<Product> {
  const response = await apiFetch(`/api/products/${encodeURIComponent(id)}`);
  if (!response.ok) throw await readApiError(response, 'Не вдалося знайти товар.');
  return await response.json() as Product;
}
