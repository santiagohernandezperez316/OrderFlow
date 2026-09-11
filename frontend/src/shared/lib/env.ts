export const env = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:8080",
  hubUrl: import.meta.env.VITE_HUB_URL ?? "http://localhost:8080/hubs/orders",
} as const;
