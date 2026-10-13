const apiBaseUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:8081';
const sessionStorageKey = 'minibox.session';
let refreshInFlight: Promise<boolean> | undefined;

export async function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const response = await send(path, init);
  if (response.status !== 401 || path.startsWith('/api/auth/')) return response;
  refreshInFlight ??= refreshSession().finally(() => { refreshInFlight = undefined; });
  if (!await refreshInFlight) return response;
  return send(path, init);
}

async function send(path: string, init: RequestInit): Promise<Response> {
  const headers = new Headers(init.headers);
  if (init.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json');
  try {
    const session = JSON.parse(localStorage.getItem(sessionStorageKey) ?? 'null') as { accessToken?: string } | null;
    if (session?.accessToken) headers.set('Authorization', `Bearer ${session.accessToken}`);
  } catch { localStorage.removeItem(sessionStorageKey); }
  return fetch(`${apiBaseUrl}${path}`, { ...init, headers });
}

async function refreshSession(): Promise<boolean> {
  try {
    const session = JSON.parse(localStorage.getItem(sessionStorageKey) ?? 'null') as { refreshToken?: string } | null;
    if (!session?.refreshToken) return false;
    const response = await fetch(`${apiBaseUrl}/api/auth/refresh`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ refreshToken: session.refreshToken }),
    });
    if (!response.ok) { localStorage.removeItem(sessionStorageKey); return false; }
    localStorage.setItem(sessionStorageKey, JSON.stringify(await response.json()));
    return true;
  } catch {
    localStorage.removeItem(sessionStorageKey);
    return false;
  }
}

export async function readApiError(response: Response, fallback: string): Promise<Error> {
  try {
    const result = await response.json() as { message?: string; title?: string; errors?: Record<string, string[]> };
    const validationMessage = Object.values(result.errors ?? {}).flat()[0];
    return new Error(result.message ?? validationMessage ?? result.title ?? fallback);
  } catch { return new Error(fallback); }
}
