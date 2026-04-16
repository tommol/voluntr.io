import { redirect } from "next/navigation";
import { getSession } from "@/lib/auth/session";
import type { Locale } from "@/lib/i18n/config";

export async function requireAuth(locale: Locale) {
  const session = await getSession();
  if (!session?.isAuthenticated) {
    redirect(`/${locale}?auth=required`);
  }

  return session;
}

export async function requireRole(locale: Locale, role: string) {
  const session = await requireAuth(locale);
  if (!session.roles.includes(role)) {
    redirect(`/${locale}/app?forbidden=1`);
  }

  return session;
}

