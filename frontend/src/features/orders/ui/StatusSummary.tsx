import type { Order, OrderStatus } from "../domain/Order";

const ORDER: OrderStatus[] = ["Pending", "Confirmed", "Rejected"];

const DOT_COLOR: Record<OrderStatus, string> = {
  Pending: "bg-accent-pending",
  Confirmed: "bg-accent-confirmed",
  Rejected: "bg-accent-rejected",
};

const LABELS: Record<OrderStatus, string> = {
  Pending: "pendientes",
  Confirmed: "confirmados",
  Rejected: "rechazados",
};

export function StatusSummary({ orders }: { orders: Order[] }) {
  if (orders.length === 0) return null;

  const counts = orders.reduce<Record<OrderStatus, number>>(
    (acc, order) => {
      acc[order.status] += 1;
      return acc;
    },
    { Pending: 0, Confirmed: 0, Rejected: 0 },
  );

  return (
    <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-ink/60">
      {ORDER.map((status) => (
        <span key={status} className="inline-flex items-center gap-1.5">
          <span className={`h-1.5 w-1.5 rounded-full ${DOT_COLOR[status]}`} />
          {counts[status]} {LABELS[status]}
        </span>
      ))}
    </div>
  );
}
