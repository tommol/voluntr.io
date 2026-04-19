import { cookies } from "next/headers";
import type { OrganizationMembership } from "@/lib/api/organizations";

export const ACTIVE_ORG_COOKIE = "active-org-slug";
const ONE_DAY = 60 * 60 * 24;

export async function getActiveOrgSlug(): Promise<string | null> {
  const cookieStore = await cookies();
  return cookieStore.get(ACTIVE_ORG_COOKIE)?.value ?? null;
}

export function resolveActiveOrg(
  memberships: OrganizationMembership[],
  slug: string | null,
): OrganizationMembership | null {
  if (!memberships.length) return null;
  if (slug) {
    const match = memberships.find((m) => m.slug === slug);
    if (match) return match;
  }
  if (memberships.length === 1) return memberships[0];
  return null;
}

export function buildSetOrgCookieHeaders(slug: string): Record<string, string> {
  return {
    "Set-Cookie": `${ACTIVE_ORG_COOKIE}=${encodeURIComponent(slug)}; Path=/; SameSite=Lax; Max-Age=${ONE_DAY}; HttpOnly`,
  };
}
