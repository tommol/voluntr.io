import { cookies } from "next/headers";

export interface Session {
  isAuthenticated: boolean;
  userId?: string;
  roles: string[];
}

export const sessionCookieName = "voluntr_session";
export const userIdCookieName = "voluntr_user";
export const rolesCookieName = "voluntr_roles";

interface CookieReader {
  get(name: string): { value: string } | undefined;
}

function parseRoles(rawRoles: string | undefined): string[] {
  if (!rawRoles) {
    return [];
  }

  return rawRoles
    .split(",")
    .map((role) => role.trim())
    .filter(Boolean);
}

export function getSessionFromCookies(cookieStore: CookieReader): Session | null {
  const isAuthenticated = cookieStore.get(sessionCookieName)?.value === "1";
  if (!isAuthenticated) {
    return null;
  }

  return {
    isAuthenticated: true,
    userId: cookieStore.get(userIdCookieName)?.value,
    roles: parseRoles(cookieStore.get(rolesCookieName)?.value),
  };
}

export async function getSession(): Promise<Session | null> {
  const cookieStore = await cookies();
  return getSessionFromCookies(cookieStore);
}

