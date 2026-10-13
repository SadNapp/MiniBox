import { apiFetch, readApiError } from './http-client';
import type { CreateOrderInput, OrderReceipt, UserOrder } from '../types/order';

export interface AuthResult { userId: string; email: string; firstName?: string; lastName?: string; accessToken: string; refreshToken: string; expiresAtUtc: string; refreshExpiresAtUtc: string; }

export async function login(email: string, password: string): Promise<AuthResult> {
  const response = await apiFetch('/api/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) });
  if (!response.ok) throw await readApiError(response, 'Не вдалося увійти.');
  return await response.json() as AuthResult;
}

export async function register(input: { firstName: string; lastName: string; email: string; password: string }): Promise<AuthResult> {
  const response = await apiFetch('/api/auth/register', { method: 'POST', body: JSON.stringify(input) });
  if (!response.ok) throw await readApiError(response, 'Не вдалося створити обліковий запис.');
  return await response.json() as AuthResult;
}

export async function createOrder(input: CreateOrderInput): Promise<OrderReceipt> {
  const response = await apiFetch('/api/orders', { method: 'POST', body: JSON.stringify(input) });
  if (!response.ok) throw await readApiError(response, 'Не вдалося оформити замовлення.');
  return await response.json() as OrderReceipt;
}

export async function getMyOrders(): Promise<UserOrder[]> {
  const response = await apiFetch('/api/orders/my');
  if (!response.ok) throw await readApiError(response, 'Не вдалося завантажити замовлення.');
  return await response.json() as UserOrder[];
}
