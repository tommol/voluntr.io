import { redirect } from "next/navigation";
import { headers } from "next/headers";
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
    const referer = (await headers()).get("referer") ?? "";
    if (referer.includes("/api/auth/kinde_callback")) {
      redirect(`/${locale}/signed-out`);
    }

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
    redirect(`/${locale}/signed-out`);
  }

  const membershipsResult = await getMyOrganizations(session.accessToken);
  if (membershipsResult.requiresReauth) {
    // Avoid login redirect loops when API rejects access token (401).
    // In this case the issue is usually token audience/scope/configuration.
    redirect(`/${locale}/signed-out`);
  }

  if (!membershipsResult.hasMembership) {
    redirect(`/${locale}/app/no-organization`);
  }
}
