import { Button } from "../../../shared/ui/Button";
import type { OrdersListStatus, RealtimeMode } from "../application/useOrdersList";
import type { Order, OrderStatus } from "../domain/Order";
import { CopyableId } from "./CopyableId";
import { LiveIndicator } from "./LiveIndicator";
import { StatusPill } from "./StatusPill";
import { StatusSummary } from "./StatusSummary";

interface OrdersListProps {
  status: OrdersListStatus;
  errorMessage: string | null;
  orders: Order[];
  mode: RealtimeMode;
  flashingIds: Set<string>;
  onRetry: () => void;
}

const FLASH_BACKGROUND: Record<OrderStatus, string> = {
  Pending: "bg-accent-pending/10",
  Confirmed: "bg-accent-confirmed/10",
  Rejected: "bg-accent-rejected/10",
};

const COLUMNS = ["Id", "Cliente", "SKU", "Cantidad", "Estado", "Creado"] as const;

function formatTimestamp(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return iso;
  return date.toLocaleString("es-CO", {
    dateStyle: "short",
    timeStyle: "medium",
  });
}

function TableHead() {
  return (
    <thead>
      <tr className="border-b border-line text-xs uppercase tracking-wide text-ink/50">
        {COLUMNS.map((column) => (
          <th key={column} className="py-2 pr-4 font-medium">
            {column}
          </th>
        ))}
      </tr>
    </thead>
  );
}

function SkeletonRows() {
  return (
    <div className="overflow-x-auto">
      <table className="w-full min-w-[560px] border-collapse text-left text-sm">
        <TableHead />
        <tbody>
          {[0, 1, 2, 3].map((row) => (
            <tr key={row} className="border-b border-line">
              {COLUMNS.map((column) => (
                <td key={column} className="py-3 pr-4">
                  <div className="h-3 w-4/5 max-w-24 bg-line/60" />
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function EmptyState() {
  return (
    <div className="flex flex-col items-start gap-1 border border-dashed border-line px-4 py-6">
      <p className="text-sm text-ink">Sin pedidos todavía</p>
      <p className="text-xs text-ink/50">Los que crees en el panel de la izquierda aparecerán aquí en tiempo real.</p>
    </div>
  );
}

export function OrdersList({ status, errorMessage, orders, mode, flashingIds, onRetry }: OrdersListProps) {
  return (
    <section className="flex flex-1 flex-col gap-4">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-medium uppercase tracking-wide text-ink/70">
          Pedidos{orders.length > 0 ? ` (${orders.length})` : ""}
        </h2>
        <LiveIndicator mode={mode} />
      </div>

      <StatusSummary orders={orders} />

      {(status === "loading" || status === "idle") && orders.length === 0 ? <SkeletonRows /> : null}

      {status === "error" ? (
        <div
          role="alert"
          className="flex items-center justify-between border border-accent-rejected px-3 py-2 text-sm text-accent-rejected"
        >
          <span>{errorMessage}</span>
          <Button className="w-auto px-3 py-1 text-xs" onClick={onRetry}>
            Reintentar
          </Button>
        </div>
      ) : null}

      {status === "success" && orders.length === 0 ? <EmptyState /> : null}

      {orders.length > 0 ? (
        <div className="overflow-x-auto">
          <table className="w-full min-w-[560px] border-collapse text-left text-sm">
            <TableHead />
            <tbody>
              {orders.map((order) => (
                <tr
                  key={order.id}
                  className={`border-b border-line transition-colors duration-500 ${
                    flashingIds.has(order.id) ? FLASH_BACKGROUND[order.status] : ""
                  }`}
                >
                  <td className="py-2 pr-4">
                    <CopyableId id={order.id} />
                  </td>
                  <td className="py-2 pr-4">{order.clienteNombre}</td>
                  <td className="py-2 pr-4 font-mono text-xs">{order.sku}</td>
                  <td className="py-2 pr-4 tabular-nums">{order.cantidad}</td>
                  <td className="py-2 pr-4">
                    <StatusPill status={order.status} />
                  </td>
                  <td className="py-2 pr-4 font-mono text-xs text-ink/70">{formatTimestamp(order.creadoEn)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}
    </section>
  );
}
