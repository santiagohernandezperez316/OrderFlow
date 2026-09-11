import { HttpOrderRepository } from "../features/orders/infrastructure/httpOrderRepository";
import { SignalROrdersAdapter } from "../features/orders/infrastructure/signalrOrdersAdapter";

export const container = {
  orderRepository: new HttpOrderRepository(),
  orderRealtime: new SignalROrdersAdapter(),
};
