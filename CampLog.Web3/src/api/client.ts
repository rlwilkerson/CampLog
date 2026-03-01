const API_BASE = import.meta.env.VITE_API_BASE_URL ?? 'https://api';

async function getToken(): Promise<string | null> {
  // Token is injected by the OIDC context — components pass it via headers
  // We use a module-level setter pattern for simplicity
  return tokenStore.token;
}

const tokenStore = {
  token: null as string | null,
  setToken(t: string | null) { this.token = t; },
};

export const setApiToken = (token: string | null) => tokenStore.setToken(token);

async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const token = await getToken();
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(init?.headers ?? {}),
  };
  const res = await fetch(`${API_BASE}${path}`, { ...init, headers });
  if (!res.ok) {
    const text = await res.text().catch(() => '');
    throw new Error(`API ${res.status}: ${text}`);
  }
  if (res.status === 204) return undefined as T;
  return res.json();
}

export const api = {
  trips: {
    list: () => apiFetch<import('../types/api').Trip[]>('/trips'),
    get: (id: string) => apiFetch<import('../types/api').Trip>(`/trips/${id}`),
    create: (data: import('../types/api').CreateTripRequest) =>
      apiFetch<import('../types/api').Trip>('/trips', { method: 'POST', body: JSON.stringify(data) }),
    update: (id: string, data: import('../types/api').UpdateTripRequest) =>
      apiFetch<import('../types/api').Trip>(`/trips/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id: string) => apiFetch<void>(`/trips/${id}`, { method: 'DELETE' }),
  },
  locations: {
    list: (tripId: string) => apiFetch<import('../types/api').Location[]>(`/trips/${tripId}/locations`),
    get: (tripId: string, id: string) => apiFetch<import('../types/api').Location>(`/trips/${tripId}/locations/${id}`),
    create: (tripId: string, data: import('../types/api').CreateLocationRequest) =>
      apiFetch<import('../types/api').Location>(`/trips/${tripId}/locations`, { method: 'POST', body: JSON.stringify(data) }),
    update: (tripId: string, id: string, data: import('../types/api').UpdateLocationRequest) =>
      apiFetch<import('../types/api').Location>(`/trips/${tripId}/locations/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (tripId: string, id: string) =>
      apiFetch<void>(`/trips/${tripId}/locations/${id}`, { method: 'DELETE' }),
  },
};
