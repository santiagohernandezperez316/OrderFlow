import { useCallback, useEffect, useRef, useState } from "react";
import type { OrderStatusChangedHandler } from "../features/orders/application/useOrdersList";
import { useCreateOrder } from "../features/orders/application/useCreateOrder";
import { useOrdersList } from "../features/orders/application/useOrdersList";
import { KNOWN_SKUS } from "../features/orders/domain/Order";
import { OrderForm } from "../features/orders/ui/OrderForm";
import { OrdersList } from "../features/orders/ui/OrdersList";
import { Toast } from "../shared/ui/Toast";
import { container } from "./container";

const REJECTION_TOAST_DURATION_MS = 6_000;

export function App() {
  const createOrder = useCreateOrder(container.orderRepository);
  const myOrderIdRef = useRef<string | null>(null);
  const [rejectionToast, setRejectionToast] = useState<string | null>(null);
  const toastTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const dismissToast = useCallback(() => {
    if (toastTimeoutRef.current) {
      clearTimeout(toastTimeoutRef.current);
      toastTimeoutRef.current = null;
    }
    setRejectionToast(null);
  }, []);

  const handleOrderStatusChanged = useCallback<OrderStatusChangedHandler>((orderId, status, reason) => {
    if (orderId !== myOrderIdRef.current || status !== "Rejected") return;
    myOrderIdRef.current = null;
    setRejectionToast(`Rechazado: ${reason ?? "stock insuficiente"}`);
    if (toastTimeoutRef.current) clearTimeout(toastTimeoutRef.current);
    toastTimeoutRef.current = setTimeout(() => setRejectionToast(null), REJECTION_TOAST_DURATION_MS);
  }, []);

  useEffect(() => {
    return () => {
      if (toastTimeoutRef.current) clearTimeout(toastTimeoutRef.current);
    };
  }, []);

  const ordersList = useOrdersList(container.orderRepository, container.orderRealtime, handleOrderStatusChanged);

  async function handleCreateOrder(input: Parameters<typeof createOrder.submit>[0]): Promise<boolean> {
    const created = await createOrder.submit(input);
    if (created) {
      myOrderIdRef.current = created.id;
      ordersList.retry();
    }
    return created !== null;
  }

  return (
    <div className="mx-auto flex min-h-screen max-w-6xl flex-col gap-6 px-4 py-6 sm:gap-8 sm:px-6 sm:py-10 md:flex-row">
      <aside className="w-full shrink-0 md:sticky md:top-10 md:h-fit md:w-[380px]">
        <OrderForm
          skus={KNOWN_SKUS}
          status={createOrder.status}
          fieldErrors={createOrder.fieldErrors}
          bannerMessage={createOrder.bannerMessage}
          onSubmit={handleCreateOrder}
        />
      </aside>

      <main className="flex-1">
        <OrdersList
          status={ordersList.status}
          errorMessage={ordersList.errorMessage}
          orders={ordersList.orders}
          mode={ordersList.mode}
          flashingIds={ordersList.flashingIds}
          onRetry={ordersList.retry}
        />
      </main>

      {rejectionToast ? <Toast message={rejectionToast} onDismiss={dismissToast} /> : null}
    </div>
  );
}
