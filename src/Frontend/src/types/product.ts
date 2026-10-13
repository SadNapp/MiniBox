export interface Product {
  id: string;
  sku?: string;
  name: string;
  description: string;
  price: number;
  stock: number;
}

export interface ProductPage {
  items: Product[];
  page: number;
  pageSize: number;
  totalCount: number;
}
