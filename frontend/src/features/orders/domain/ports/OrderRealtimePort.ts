import type { OrderStatus } from "../Order";

export type RealtimeConnectionState = "connecting" | "connected" | "reconnecting" | "disconnected";

export type OrderStatusChangedListener = (orderId: string, status: OrderStatus, reason: string | null) => void;
export type ConnectionStateListener = (state: RealtimeConnectionState) => void;

export interface OrderRealtimePort {
  start(onStatusChanged: OrderStatusChangedListener, onConnectionStateChange: ConnectionStateListener): void;
  stop(): Promise<void>;
}
