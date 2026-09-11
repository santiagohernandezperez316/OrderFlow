import { useState } from "react";

export function CopyableId({ id }: { id: string }) {
  const [copied, setCopied] = useState(false);

  async function handleCopy() {
    try {
      await navigator.clipboard.writeText(id);
      setCopied(true);
      setTimeout(() => setCopied(false), 1200);
    } catch {
      // Clipboard API unavailable (e.g. insecure context); nothing to show.
    }
  }

  return (
    <button
      type="button"
      onClick={handleCopy}
      title={id}
      className="font-mono text-xs text-ink/70 underline decoration-line decoration-dotted underline-offset-4 hover:text-ink hover:decoration-ink"
    >
      {copied ? "Copiado" : id.slice(0, 8)}
    </button>
  );
}
