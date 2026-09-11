import type { CreateOrderInput, Order } from "../Order";

export interface OrderRepositoryPort {
  list(signal?: AbortSignal): Promise<Order[]>;
  create(input: CreateOrderInput): Promise<Order>;
}
