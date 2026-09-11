import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import { env } from "../../../shared/lib/env";
import type { OrderStatus } from "../domain/Order";
import type {
  ConnectionStateListener,
  OrderRealtimePort,
  OrderStatusChangedListener,
} from "../domain/ports/OrderRealtimePort";

export class SignalROrdersAdapter implements OrderRealtimePort {
  private connection: HubConnection | null = null;

  start(onStatusChanged: OrderStatusChangedListener, onConnectionStateChange: ConnectionStateListener): void {
    const connection = new HubConnectionBuilder()
      .withUrl(env.hubUrl)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on("OrderStatusChanged", (orderId: string, status: OrderStatus, reason: string | null) => {
      onStatusChanged(orderId, status, reason);
    });

    connection.onreconnecting(() => onConnectionStateChange("reconnecting"));
    connection.onreconnected(() => onConnectionStateChange("connected"));
    connection.onclose(() => onConnectionStateChange("disconnected"));

    this.connection = connection;
    onConnectionStateChange("connecting");

    connection
      .start()
      .then(() => onConnectionStateChange("connected"))
      .catch(() => onConnectionStateChange("disconnected"));
  }

  async stop(): Promise<void> {
    if (this.connection && this.connection.state !== HubConnectionState.Disconnected) {
      await this.connection.stop();
    }
    this.connection = null;
  }
}
