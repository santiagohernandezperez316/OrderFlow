import { useCallback, useEffect, useRef, useState } from "react";
import type { Order, OrderStatus } from "../domain/Order";
import { OrderApiException } from "../domain/errors";
import type { OrderRepositoryPort } from "../domain/ports/OrderRepositoryPort";
import type { OrderRealtimePort, RealtimeConnectionState } from "../domain/ports/OrderRealtimePort";

export type OrderStatusChangedHandler = (orderId: string, status: OrderStatus, reason: string | null) => void;

const POLLING_INTERVAL_MS = 10_000;
const FLASH_DURATION_MS = 500;

export type OrdersListStatus = "loading" | "success" | "error";
export type RealtimeMode = "live" | "polling";

export interface UseOrdersListResult {
  status: OrdersListStatus;
  errorMessage: string | null;
  orders: Order[];
  mode: RealtimeMode;
  flashingIds: Set<string>;
  retry: () => void;
}

function describeListError(error: unknown): string {
  if (error instanceof OrderApiException) {
    return error.apiError.kind === "validation"
      ? "El servidor rechazó la solicitud."
      : error.message;
  }
  return "No se pudo cargar la lista de pedidos.";
}

function isAbortError(error: unknown): boolean {
  return error instanceof DOMException && error.name === "AbortError";
}

export function useOrdersList(
  repository: OrderRepositoryPort,
  realtime: OrderRealtimePort,
  onOrderStatusChanged?: OrderStatusChangedHandler,
): UseOrdersListResult {
  const [status, setStatus] = useState<OrdersListStatus>("loading");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [orders, setOrders] = useState<Order[]>([]);
  const [mode, setMode] = useState<RealtimeMode>("polling");
  const [flashingIds, setFlashingIds] = useState<Set<string>>(new Set());

  const pollingIntervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const abortControllerRef = useRef<AbortController | null>(null);
  const flashTimeoutsRef = useRef<Set<ReturnType<typeof setTimeout>>>(new Set());
  const isMountedRef = useRef(true);

  const fetchOrders = useCallback(
    async (showLoading: boolean) => {
      abortControllerRef.current?.abort();
      const controller = new AbortController();
      abortControllerRef.current = controller;

      if (showLoading) {
        setStatus("loading");
        setErrorMessage(null);
      }

      try {
        const result = await repository.list(controller.signal);
        if (!isMountedRef.current || controller.signal.aborted) return;
        setOrders(result);
        setStatus("success");
        setErrorMessage(null);
      } catch (error) {
        if (isAbortError(error) || controller.signal.aborted || !isMountedRef.current) return;
        setErrorMessage(describeListError(error));
        setStatus("error");
      }
    },
    [repository],
  );

  const loadOrders = useCallback(() => {
    fetchOrders(true);
  }, [fetchOrders]);

  const flashOrder = useCallback((orderId: string) => {
    setFlashingIds((current) => new Set(current).add(orderId));
    const timeoutId = setTimeout(() => {
      flashTimeoutsRef.current.delete(timeoutId);
      if (!isMountedRef.current) return;
      setFlashingIds((current) => {
        const next = new Set(current);
        next.delete(orderId);
        return next;
      });
    }, FLASH_DURATION_MS);
    flashTimeoutsRef.current.add(timeoutId);
  }, []);

  useEffect(() => {
    isMountedRef.current = true;
    return () => {
      isMountedRef.current = false;
      abortControllerRef.current?.abort();
    };
  }, []);

  useEffect(() => {
    loadOrders();
  }, [loadOrders]);

  useEffect(() => {
    function stopPolling() {
      if (pollingIntervalRef.current !== null) {
        clearInterval(pollingIntervalRef.current);
        pollingIntervalRef.current = null;
      }
    }

    function startPolling() {
      if (pollingIntervalRef.current !== null) return;
      pollingIntervalRef.current = setInterval(() => {
        fetchOrders(false);
      }, POLLING_INTERVAL_MS);
    }

    function handleConnectionStateChange(state: RealtimeConnectionState) {
      if (!isMountedRef.current) return;
      if (state === "connected") {
        setMode("live");
        stopPolling();
      } else {
        setMode("polling");
        startPolling();
      }
    }

    realtime.start((orderId, orderStatus, reason) => {
      if (!isMountedRef.current) return;
      setOrders((current) =>
        current.map((order) =>
          order.id === orderId ? { ...order, status: orderStatus, rejectionReason: reason } : order,
        ),
      );
      flashOrder(orderId);
      onOrderStatusChanged?.(orderId, orderStatus, reason);
    }, handleConnectionStateChange);

    return () => {
      stopPolling();
      realtime.stop();
      flashTimeoutsRef.current.forEach((timeoutId) => clearTimeout(timeoutId));
      flashTimeoutsRef.current.clear();
    };
  }, [realtime, fetchOrders, flashOrder, onOrderStatusChanged]);

  return { status, errorMessage, orders, mode, flashingIds, retry: loadOrders };
}
