import { env } from "./env";

export class HttpNetworkError extends Error {
  constructor(message: string) {
    super(message);
    this.name = "HttpNetworkError";
  }
}

export async function httpRequest(path: string, init?: RequestInit): Promise<Response> {
  try {
    return await fetch(`${env.apiBaseUrl}${path}`, {
      ...init,
      headers: {
        "Content-Type": "application/json",
        ...init?.headers,
      },
    });
  } catch (error) {
    if (error instanceof DOMException && error.name === "AbortError") {
      throw error;
    }
    throw new HttpNetworkError(
      "No se pudo conectar con el servidor. Verifica tu conexión e intenta de nuevo.",
    );
  }
}
