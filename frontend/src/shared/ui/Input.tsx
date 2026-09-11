import type { InputHTMLAttributes } from "react";

type InputProps = InputHTMLAttributes<HTMLInputElement> & { invalid?: boolean };

export function Input({ className = "", invalid, ...props }: InputProps) {
  return (
    <input
      className={`w-full border bg-canvas px-3 py-2 text-base text-ink outline-none transition-colors focus:border-ink focus:ring-1 focus:ring-ink/20 sm:text-sm ${
        invalid ? "border-accent-rejected" : "border-line"
      } ${className}`}
      {...props}
    />
  );
}
