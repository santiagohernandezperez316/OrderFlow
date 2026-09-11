export type OrderStatus = "Pending" | "Confirmed" | "Rejected";

export interface Order {
  id: string;
  clienteNombre: string;
  sku: string;
  cantidad: number;
  status: OrderStatus;
  creadoEn: string;
  rejectionReason: string | null;
}

export const KNOWN_SKUS = ["SKU-001", "SKU-002", "SKU-003"] as const;

export interface CreateOrderInput {
  clienteNombre: string;
  sku: string;
  cantidad: number;
}
