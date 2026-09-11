import { HttpNetworkError, httpRequest } from "../../../shared/lib/httpClient";
import { OrderApiException } from "../domain/errors";
import type { CreateOrderInput, Order } from "../domain/Order";
import type { OrderRepositoryPort } from "../domain/ports/OrderRepositoryPort";

interface ValidationProblemBody {
  errors: Record<string, string[]>;
}

interface BusinessErrorBody {
  ErrorMessage: string;
}

function isValidationProblemBody(body: unknown): body is ValidationProblemBody {
  return typeof body === "object" && body !== null && "errors" in body;
}

function isBusinessErrorBody(body: unknown): body is BusinessErrorBody {
  return (
    typeof body === "object" &&
    body !== null &&
    "ErrorMessage" in body &&
    typeof (body as Record<string, unknown>).ErrorMessage === "string"
  );
}

const SERVER_ERROR_MESSAGE = "Ocurrió un error en el servidor, intenta de nuevo.";

async function throwForErrorResponse(response: Response): Promise<never> {
  if (response.status !== 400) {
    throw new OrderApiException({ kind: "network", message: SERVER_ERROR_MESSAGE });
  }

  let body: unknown;
  try {
    body = await response.json();
  } catch {
    throw new OrderApiException({ kind: "network", message: SERVER_ERROR_MESSAGE });
  }

  if (isValidationProblemBody(body)) {
    throw new OrderApiException({ kind: "validation", fieldErrors: body.errors });
  }

  if (isBusinessErrorBody(body)) {
    throw new OrderApiException({ kind: "business", message: body.ErrorMessage });
  }

  throw new OrderApiException({ kind: "network", message: SERVER_ERROR_MESSAGE });
}

export class HttpOrderRepository implements OrderRepositoryPort {
  async list(signal?: AbortSignal): Promise<Order[]> {
    let response: Response;
    try {
      response = await httpRequest("/orders", { signal });
    } catch (error) {
      if (error instanceof HttpNetworkError) {
        throw new OrderApiException({ kind: "network", message: error.message });
      }
      throw error;
    }

    if (!response.ok) {
      await throwForErrorResponse(response);
    }

    return (await response.json()) as Order[];
  }

  async create(input: CreateOrderInput): Promise<Order> {
    let response: Response;
    try {
      response = await httpRequest("/orders", {
        method: "POST",
        body: JSON.stringify(input),
      });
    } catch (error) {
      if (error instanceof HttpNetworkError) {
        throw new OrderApiException({ kind: "network", message: error.message });
      }
      throw error;
    }

    if (!response.ok) {
      await throwForErrorResponse(response);
    }

    return (await response.json()) as Order;
  }
}
