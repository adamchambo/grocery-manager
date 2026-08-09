const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5080";

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly details: unknown,
  ) {
    super(`API request failed with status ${status}.`);
    this.name = "ApiError";
  }
}

export async function apiFetch<T>(
  url: string,
  options: RequestInit,
): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${url}`, {
    ...options,
    credentials: "include",
    headers: {
      Accept: "application/json",
      ...options.headers,
    },
  });

  if (!response.ok) {
    const details = await response.json().catch(() => null);
    throw new ApiError(response.status, details);
  }

  if (response.status === 204) return undefined as T;

  return (await response.json()) as T;
}

export async function apiFetchBlob(url: string, options: RequestInit): Promise<Blob> {
  const response = await fetch(`${apiBaseUrl}${url}`, {
    ...options,
    credentials: "include",
    headers: { Accept: "application/pdf", ...options.headers },
  });
  if (!response.ok) {
    const details = await response.json().catch(() => null);
    throw new ApiError(response.status, details);
  }
  return response.blob();
}
