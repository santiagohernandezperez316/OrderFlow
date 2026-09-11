import { useState, type FormEvent } from "react";
import { Button } from "../../../shared/ui/Button";
import { Field } from "../../../shared/ui/Field";
import { Input } from "../../../shared/ui/Input";
import { Select } from "../../../shared/ui/Select";
import type { CreateOrderStatus } from "../application/useCreateOrder";
import type { CreateOrderInput } from "../domain/Order";

interface OrderFormProps {
  skus: readonly string[];
  status: CreateOrderStatus;
  fieldErrors: Record<string, string[]>;
  bannerMessage: string | null;
  onSubmit: (input: CreateOrderInput) => Promise<boolean>;
}

export function OrderForm({ skus, status, fieldErrors, bannerMessage, onSubmit }: OrderFormProps) {
  const [clienteNombre, setClienteNombre] = useState("");
  const [sku, setSku] = useState(skus[0] ?? "");
  const [cantidad, setCantidad] = useState("1");

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    const succeeded = await onSubmit({
      clienteNombre,
      sku,
      cantidad: Number.parseInt(cantidad, 10),
    });
    if (succeeded) {
      setClienteNombre("");
      setSku(skus[0] ?? "");
      setCantidad("1");
    }
  }

  const isLoading = status === "loading";

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      <h2 className="text-sm font-medium uppercase tracking-wide text-ink/70">Nuevo pedido</h2>

      <Field label="Cliente" htmlFor="clienteNombre" error={fieldErrors.ClienteNombre?.[0]}>
        <Input
          id="clienteNombre"
          value={clienteNombre}
          onChange={(event) => setClienteNombre(event.target.value)}
          invalid={Boolean(fieldErrors.ClienteNombre)}
          placeholder="Nombre del cliente"
          autoFocus
        />
      </Field>

      <Field label="SKU" htmlFor="sku" error={fieldErrors.Sku?.[0]}>
        <Select
          id="sku"
          value={sku}
          onChange={(event) => setSku(event.target.value)}
          invalid={Boolean(fieldErrors.Sku)}
          className="font-mono"
        >
          {skus.map((value) => (
            <option key={value} value={value}>
              {value}
            </option>
          ))}
        </Select>
      </Field>

      <Field label="Cantidad" htmlFor="cantidad" error={fieldErrors.Cantidad?.[0]}>
        <Input
          id="cantidad"
          type="number"
          min={1}
          max={100}
          value={cantidad}
          onChange={(event) => setCantidad(event.target.value)}
          invalid={Boolean(fieldErrors.Cantidad)}
        />
      </Field>

      <div aria-live="polite">
        {bannerMessage ? (
          <p role="alert" className="border border-accent-rejected px-3 py-2 text-xs text-accent-rejected">
            {bannerMessage}
          </p>
        ) : null}

        {status === "success" ? (
          <p className="border border-accent-confirmed px-3 py-2 text-xs text-accent-confirmed">
            Pedido creado correctamente.
          </p>
        ) : null}
      </div>

      <Button type="submit" disabled={isLoading}>
        {isLoading ? "Creando pedido..." : "Crear pedido"}
      </Button>
    </form>
  );
}
