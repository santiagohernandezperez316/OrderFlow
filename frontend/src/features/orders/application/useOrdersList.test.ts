import { act, renderHook } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import type { Order, OrderStatus } from "../domain/Order";
import { OrderApiException } from "../domain/errors";
import type { OrderRepositoryPort } from "../domain/ports/OrderRepositoryPort";
import type {
  ConnectionStateListener,
  OrderRealtimePort,
  OrderStatusChangedListener,
  RealtimeConnectionState,
} from "../domain/ports/OrderRealtimePort";
import { useOrdersList } from "./useOrdersList";

const POLLING_INTERVAL_MS = 10_000;

function makeOrder(overrides: Partial<Order> = {}): Order {
  return {
    id: "order-1",
    clienteNombre: "Ana",
    sku: "SKU-001",
    cantidad: 5,
    status: "Pending",
    creadoEn: new Date().toISOString(),
    rejectionReason: null,
    ...overrides,
  };
}

function createRealtimeMock() {
  let statusListener: OrderStatusChangedListener | null = null;
  let connectionListener: ConnectionStateListener | null = null;

  const port: OrderRealtimePort = {
    start: vi.fn((onStatusChanged, onConnectionStateChange) => {
      statusListener = onStatusChanged;
      connectionListener = onConnectionStateChange;
    }),
    stop: vi.fn().mockResolvedValue(undefined),
  };

  return {
    port,
    emitConnectionState: (state: RealtimeConnectionState) => connectionListener?.(state),
    emitStatusChanged: (orderId: string, status: OrderStatus, reason: string | null = null) =>
      statusListener?.(orderId, status, reason),
  };
}

afterEach(() => {
  vi.useRealTimers();
});

describe("useOrdersList", () => {
  it("goes from loading to success once the initial list resolves", async () => {
    const orders = [makeOrder()];
    const repository: OrderRepositoryPort = {
      list: vi.fn().mockResolvedValue(orders),
      create: vi.fn(),
    };
    const { port: realtime } = createRealtimeMock();

    const { result } = renderHook(() => useOrdersList(repository, realtime));

    expect(result.current.status).toBe("loading");

    await act(async () => {
      await Promise.resolve();
    });

    expect(result.current.status).toBe("success");
    expect(result.current.orders).toEqual(orders);
    expect(result.current.errorMessage).toBeNull();
  });

  it("reflects an initial load failure in status/errorMessage", async () => {
    const repository: OrderRepositoryPort = {
      list: vi.fn().mockRejectedValue(new OrderApiException({ kind: "business", message: "El servidor falló." })),
      create: vi.fn(),
    };
    const { port: realtime } = createRealtimeMock();

    const { result } = renderHook(() => useOrdersList(repository, realtime));

    await act(async () => {
      await Promise.resolve();
    });

    expect(result.current.status).toBe("error");
    expect(result.current.errorMessage).toBe("El servidor falló.");
  });

  it("only polls while the hub is not Connected, and stops polling once Connected", async () => {
    vi.useFakeTimers();

    const repository: OrderRepositoryPort = {
      list: vi.fn().mockResolvedValue([]),
      create: vi.fn(),
    };
    const { port: realtime, emitConnectionState } = createRealtimeMock();

    const { result } = renderHook(() => useOrdersList(repository, realtime));

    await act(async () => {
      await Promise.resolve();
    });
    expect(repository.list).toHaveBeenCalledTimes(1);
    expect(result.current.mode).toBe("polling");

    act(() => {
      emitConnectionState("connecting");
    });
    expect(result.current.mode).toBe("polling");

    await act(async () => {
      await vi.advanceTimersByTimeAsync(POLLING_INTERVAL_MS);
    });
    expect(repository.list).toHaveBeenCalledTimes(2);

    act(() => {
      emitConnectionState("connected");
    });
    expect(result.current.mode).toBe("live");

    await act(async () => {
      await vi.advanceTimersByTimeAsync(POLLING_INTERVAL_MS);
    });
    expect(repository.list).toHaveBeenCalledTimes(2);
  });

  it("surfaces a polling failure the same way as an initial load failure", async () => {
    vi.useFakeTimers();

    let callCount = 0;
    const repository: OrderRepositoryPort = {
      list: vi.fn().mockImplementation(async () => {
        callCount += 1;
        if (callCount === 1) return [];
        throw new OrderApiException({ kind: "business", message: "Fallo el polling." });
      }),
      create: vi.fn(),
    };
    const { port: realtime, emitConnectionState } = createRealtimeMock();

    const { result } = renderHook(() => useOrdersList(repository, realtime));

    await act(async () => {
      await Promise.resolve();
    });
    expect(result.current.status).toBe("success");

    act(() => {
      emitConnectionState("disconnected");
    });

    await act(async () => {
      await vi.advanceTimersByTimeAsync(POLLING_INTERVAL_MS);
    });

    expect(result.current.status).toBe("error");
    expect(result.current.errorMessage).toBe("Fallo el polling.");
  });
});
