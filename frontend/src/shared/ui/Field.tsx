import type { ReactNode } from "react";

interface FieldProps {
  label: string;
  htmlFor: string;
  error?: string;
  children: ReactNode;
}

export function Field({ label, htmlFor, error, children }: FieldProps) {
  return (
    <div className="flex flex-col gap-1">
      <label htmlFor={htmlFor} className="text-xs font-medium uppercase tracking-wide text-ink/70">
        {label}
      </label>
      {children}
      {error ? <p className="text-xs text-accent-rejected">{error}</p> : null}
    </div>
  );
}
