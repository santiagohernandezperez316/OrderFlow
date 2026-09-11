interface ToastProps {
  message: string;
  onDismiss: () => void;
}

export function Toast({ message, onDismiss }: ToastProps) {
  return (
    <div
      role="alert"
      className="fixed inset-x-4 top-4 z-50 flex items-start justify-between gap-3 border border-accent-rejected bg-canvas px-4 py-3 text-sm text-accent-rejected shadow-lg sm:inset-x-auto sm:right-4 sm:max-w-sm"
    >
      <span>{message}</span>
      <button
        type="button"
        onClick={onDismiss}
        aria-label="Cerrar notificación"
        className="shrink-0 text-accent-rejected/70 hover:text-accent-rejected"
      >
        ✕
      </button>
    </div>
  );
}
