export interface UserSession {
  userId: string;
  email: string;
  firstName?: string;
  lastName?: string;
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  refreshExpiresAtUtc: string;
}

const key = 'minibox.session';

class UserStore {
  get session(): UserSession | null {
    try { return JSON.parse(localStorage.getItem(key) ?? 'null') as UserSession | null; }
    catch { localStorage.removeItem(key); return null; }
  }

  setSession(session: UserSession): void { localStorage.setItem(key, JSON.stringify(session)); }
  clear(): void { localStorage.removeItem(key); }
}

export const userStore = new UserStore();
