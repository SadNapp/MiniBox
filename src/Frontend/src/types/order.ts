export interface OrderLineInput { productId: string; quantity: number; }
export interface CreateOrderInput { customerName: string; email: string; items: OrderLineInput[]; deliveryStreet: string; deliveryCity: string; deliveryPostalCode: string; deliveryCountry: string; deliveryNotes?: string; }
export interface OrderReceipt { orderId: string; orderNumber: string; totalPrice: number; }
export interface UserOrder {
  id: string;
  orderNumber: string;
  createdAt: string;
  status: string;
  totalPrice: number;
  deliveryStreet: string;
  deliveryCity: string;
  deliveryPostalCode: string;
  deliveryCountry: string;
  deliveryNotes?: string;
  items: Array<{ productId: string; productName: string; quantity: number; unitPrice: number }>;
}
