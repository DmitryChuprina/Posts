export class ApiError extends Error {
  status: number;
  data?: object;

  constructor(status: number, message: string, data?: object) {
    super(message);
    this.status = status;
    this.data = data;
  }
}

export async function apiFetch<T = unknown>(
  url: string,
  options: RequestInit = {}
): Promise<T> {
  let res: Response;
  try {
    res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}${url}`, {
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
        ...(options.headers || {}),
      },
      ...options,
    });
  } catch {
    throw new ApiError(0, "Connection issue");
  }

  const body = await res.json().
    catch(() => null);

  if (!res.ok) {
    throw new ApiError(
      res.status,
      body?.message || "Unknown error",
      body
    );
  }

  return (body ?? {}) as T;
}