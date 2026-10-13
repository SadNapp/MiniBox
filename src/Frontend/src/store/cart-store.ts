import type { Product } from '../types/product';

const storageKey = 'minibox.cart';

class CartStore {
  readonly items = new Map<string, number>();
  readonly products = new Map<string, Product>();

  constructor() {
    try {
      const saved = JSON.parse(localStorage.getItem(storageKey) ?? '[]') as Array<{ product: Product; quantity: number }>;
      for (const entry of saved) {
        if (entry.product?.id && Number.isInteger(entry.quantity) && entry.quantity > 0) {
          this.products.set(entry.product.id, entry.product);
          this.items.set(entry.product.id, entry.quantity);
        }
      }
    } catch { localStorage.removeItem(storageKey); }
  }

  add(product: Product): boolean {
    const quantity = this.items.get(product.id) ?? 0;
    if (quantity >= product.stock) return false;
    this.items.set(product.id, quantity + 1);
    this.products.set(product.id, product);
    this.save();
    return true;
  }

  remove(id: string): void { this.items.delete(id); this.products.delete(id); this.save(); }
  clear(): void { this.items.clear(); this.products.clear(); this.save(); }
  private save(): void {
    localStorage.setItem(storageKey, JSON.stringify([...this.items].map(([id, quantity]) => ({ product: this.products.get(id), quantity }))));
  }
}

export const cartStore = new CartStore();
