import { redirect } from "next/navigation";
import { getSession, type Session } from "@/lib/auth/session";
import type { Locale } from "@/lib/i18n/config";

const truthyEnvValues = new Set(["1", "true", "yes", "on"]);
const allowZoneAccessWithoutAuth = truthyEnvValues.has(
  (process.env.ALLOW_ZONE_ACCESS_WITHOUT_AUTH ?? "").trim().toLowerCase(),
);

function createBypassSession(requiredRole?: string): Session {
  return {
    isAuthenticated: true,
    userId: "dev-user",
    roles: requiredRole ? [requiredRole] : ["admin"],
  };
}

export async function requireAuth(locale: Locale) {
  if (allowZoneAccessWithoutAuth) {
    return createBypassSession();
  }

  const session = await getSession();
  if (!session?.isAuthenticated) {
    redirect(`/${locale}?auth=required`);
  }

  return session;
}

export async function requireRole(locale: Locale, role: string) {
  if (allowZoneAccessWithoutAuth) {
    return createBypassSession(role);
  }

  const session = await requireAuth(locale);
  if (!session.roles.includes(role)) {
    redirect(`/${locale}/app?forbidden=1`);
  }

  return session;
}

