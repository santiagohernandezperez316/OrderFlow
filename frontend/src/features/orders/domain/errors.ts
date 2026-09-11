export type OrderApiError =
  | { kind: "validation"; fieldErrors: Record<string, string[]> }
  | { kind: "business"; message: string }
  | { kind: "network"; message: string };

export class OrderApiException extends Error {
  readonly apiError: OrderApiError;

  constructor(apiError: OrderApiError) {
    super(apiError.kind === "validation" ? "Error de validación" : apiError.message);
    this.name = "OrderApiException";
    this.apiError = apiError;
  }
}
