import type { OrderStatus } from "../domain/Order";

const LABELS: Record<OrderStatus, string> = {
  Pending: "Pendiente",
  Confirmed: "Confirmado",
  Rejected: "Rechazado",
};

const DOT_COLOR: Record<OrderStatus, string> = {
  Pending: "bg-accent-pending",
  Confirmed: "bg-accent-confirmed",
  Rejected: "bg-accent-rejected",
};

const TEXT_COLOR: Record<OrderStatus, string> = {
  Pending: "text-accent-pending",
  Confirmed: "text-accent-confirmed",
  Rejected: "text-accent-rejected",
};

export function StatusPill({ status }: { status: OrderStatus }) {
  return (
    <span className={`inline-flex items-center gap-2 text-sm ${TEXT_COLOR[status]}`}>
      <span className={`h-1.5 w-1.5 rounded-full ${DOT_COLOR[status]}`} />
      {LABELS[status]}
    </span>
  );
}
