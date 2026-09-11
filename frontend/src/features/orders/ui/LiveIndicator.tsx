import type { RealtimeMode } from "../application/useOrdersList";

export function LiveIndicator({ mode }: { mode: RealtimeMode }) {
  const isLive = mode === "live";
  return (
    <span
      className={`inline-flex items-center gap-2 text-xs ${isLive ? "text-accent-confirmed" : "text-ink/50"}`}
    >
      <span
        className={`h-1.5 w-1.5 rounded-full ${isLive ? "bg-accent-confirmed" : "border border-ink/40"}`}
      />
      {isLive ? "En vivo" : "Actualizando cada 10s"}
    </span>
  );
}
