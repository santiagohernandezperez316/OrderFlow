import { act, renderHook } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import type { CreateOrderInput, Order } from "../domain/Order";
import { OrderApiException } from "../domain/errors";
import type { OrderRepositoryPort } from "../domain/ports/OrderRepositoryPort";
import { useCreateOrder } from "./useCreateOrder";

function createRepositoryMock(overrides?: Partial<OrderRepositoryPort>): OrderRepositoryPort {
  return {
    list: vi.fn().mockResolvedValue([]),
    create: vi.fn(),
    ...overrides,
  };
}

const validInput: CreateOrderInput = {
  clienteNombre: "Ana",
  sku: "SKU-001",
  cantidad: 5,
};

describe("useCreateOrder", () => {
  it("starts idle and moves through loading while the request is in flight", async () => {
    let resolveCreate: (() => void) | undefined;
    const repository = createRepositoryMock({
      create: vi.fn(
        () =>
          new Promise<Order>((resolve) => {
            resolveCreate = () =>
              resolve({
                id: "1",
                clienteNombre: "Ana",
                sku: "SKU-001",
                cantidad: 5,
                status: "Pending",
                creadoEn: new Date().toISOString(),
                rejectionReason: null,
              });
          }),
      ),
    });

    const { result } = renderHook(() => useCreateOrder(repository));
    expect(result.current.status).toBe("idle");

    let submitPromise!: Promise<unknown>;
    act(() => {
      submitPromise = result.current.submit(validInput);
    });

    expect(result.current.status).toBe("loading");

    await act(async () => {
      resolveCreate?.();
      await submitPromise;
    });

    expect(result.current.status).toBe("success");
  });

  it("maps a validation error from the API to per-field errors, not a banner", async () => {
    const repository = createRepositoryMock({
      create: vi.fn().mockRejectedValue(
        new OrderApiException({
          kind: "validation",
          fieldErrors: { Cantidad: ["La cantidad debe estar entre 1 y 100."] },
        }),
      ),
    });

    const { result } = renderHook(() => useCreateOrder(repository));

    await act(async () => {
      await result.current.submit(validInput);
    });

    expect(result.current.status).toBe("error");
    expect(result.current.fieldErrors).toEqual({ Cantidad: ["La cantidad debe estar entre 1 y 100."] });
    expect(result.current.bannerMessage).toBeNull();
  });

  it("maps a business error from the API to a banner message, not field errors", async () => {
    const repository = createRepositoryMock({
      create: vi.fn().mockRejectedValue(
        new OrderApiException({ kind: "business", message: "El SKU no existe." }),
      ),
    });

    const { result } = renderHook(() => useCreateOrder(repository));

    await act(async () => {
      await result.current.submit(validInput);
    });

    expect(result.current.status).toBe("error");
    expect(result.current.fieldErrors).toEqual({});
    expect(result.current.bannerMessage).toBe("El SKU no existe.");
  });
});
