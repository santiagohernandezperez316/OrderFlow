import type { SelectHTMLAttributes } from "react";

type SelectProps = SelectHTMLAttributes<HTMLSelectElement> & { invalid?: boolean };

export function Select({ className = "", invalid, children, ...props }: SelectProps) {
  return (
    <select
      className={`w-full border bg-canvas px-3 py-2 text-base text-ink outline-none transition-colors focus:border-ink focus:ring-1 focus:ring-ink/20 sm:text-sm ${
        invalid ? "border-accent-rejected" : "border-line"
      } ${className}`}
      {...props}
    >
      {children}
    </select>
  );
}
