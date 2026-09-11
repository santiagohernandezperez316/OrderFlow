import { useCallback, useState } from "react";
import type { CreateOrderInput, Order } from "../domain/Order";
import { OrderApiException } from "../domain/errors";
import type { OrderRepositoryPort } from "../domain/ports/OrderRepositoryPort";

export type CreateOrderStatus = "idle" | "loading" | "success" | "error";

export interface CreateOrderState {
  status: CreateOrderStatus;
  fieldErrors: Record<string, string[]>;
  bannerMessage: string | null;
}

export interface UseCreateOrderResult extends CreateOrderState {
  submit: (input: CreateOrderInput) => Promise<Order | null>;
  reset: () => void;
}

function validateClientSide(input: CreateOrderInput): Record<string, string[]> {
  const errors: Record<string, string[]> = {};

  if (!input.clienteNombre.trim()) {
    errors.ClienteNombre = ["El nombre del cliente es requerido."];
  }
  if (!input.sku.trim()) {
    errors.Sku = ["El SKU es requerido."];
  }
  if (!Number.isInteger(input.cantidad) || input.cantidad < 1 || input.cantidad > 100) {
    errors.Cantidad = ["La cantidad debe estar entre 1 y 100."];
  }

  return errors;
}

const initialState: CreateOrderState = { status: "idle", fieldErrors: {}, bannerMessage: null };

export function useCreateOrder(repository: OrderRepositoryPort): UseCreateOrderResult {
  const [state, setState] = useState<CreateOrderState>(initialState);

  const submit = useCallback(
    async (input: CreateOrderInput): Promise<Order | null> => {
      const clientErrors = validateClientSide(input);
      if (Object.keys(clientErrors).length > 0) {
        setState({ status: "error", fieldErrors: clientErrors, bannerMessage: null });
        return null;
      }

      setState({ status: "loading", fieldErrors: {}, bannerMessage: null });

      try {
        const order = await repository.create(input);
        setState({ status: "success", fieldErrors: {}, bannerMessage: null });
        return order;
      } catch (error) {
        if (error instanceof OrderApiException) {
          if (error.apiError.kind === "validation") {
            setState({ status: "error", fieldErrors: error.apiError.fieldErrors, bannerMessage: null });
          } else {
            setState({ status: "error", fieldErrors: {}, bannerMessage: error.apiError.message });
          }
        } else {
          setState({
            status: "error",
            fieldErrors: {},
            bannerMessage: "Ocurrió un error inesperado al crear el pedido.",
          });
        }
        return null;
      }
    },
    [repository],
  );

  const reset = useCallback(() => setState(initialState), []);

  return { ...state, submit, reset };
}
