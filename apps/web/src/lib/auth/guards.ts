import { redirect } from "next/navigation";
import { getSession, type Session } from "@/lib/auth/session";
import type { Locale } from "@/lib/i18n/config";
import { getMyOrganizations } from "@/lib/api/organizations";

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

export async function requireAuth(locale: Locale, targetPath: string = `/${locale}/app`) {
  if (allowZoneAccessWithoutAuth) {
    return createBypassSession();
  }

  const session = await getSession();
  if (!session?.isAuthenticated) {
    const loginUrl = `/api/auth/login?post_login_redirect_url=${encodeURIComponent(targetPath)}`;
    redirect(loginUrl);
  }

  return session;
}

export async function requireRole(locale: Locale, role: string) {
  if (allowZoneAccessWithoutAuth) {
    return createBypassSession(role);
  }

  return requireAuth(locale, `/${locale}/admin`);
}

export async function requireOrganizationMembership(locale: Locale, session: Session) {
  if (allowZoneAccessWithoutAuth) {
    return;
  }

  if (!session.accessToken) {
    redirect(`/${locale}/no-organization`);
  }

  const membershipsResult = await getMyOrganizations(session.accessToken);
  if (!membershipsResult.hasMembership) {
    redirect(`/${locale}/no-organization`);
  }
}
