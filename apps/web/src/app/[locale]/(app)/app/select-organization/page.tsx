import type { Metadata } from "next";
import { t } from "@/lib/i18n/dictionaries";
import { isLocale } from "@/lib/i18n/config";
import { notFound } from "next/navigation";
import { buildAlternates } from "@/lib/seo/alternates";
import { requireAuth } from "@/lib/auth/guards";
import { getMyOrganizations } from "@/lib/api/organizations";
import { getActiveOrgSlug } from "@/lib/auth/active-org";
import { OrgSelector } from "./org-selector";
import styles from "../../../landing.module.css";

type LocaleParams = Promise<{ locale: string }>;

export async function generateMetadata({ params }: { params: LocaleParams }): Promise<Metadata> {
  const { locale } = await params;
  if (!isLocale(locale)) return {};
  return {
    title: t(locale, "selectOrganization.title"),
    alternates: buildAlternates(locale, "/app/select-organization"),
  };
}

export default async function SelectOrganizationPage({ params }: { params: LocaleParams }) {
  const { locale } = await params;
  if (!isLocale(locale)) notFound();

  const session = await requireAuth(locale, `/${locale}/app`);

  if (!session.accessToken) {
    notFound();
  }

  const membershipsResult = await getMyOrganizations(session.accessToken);

  if (!membershipsResult.hasMembership) {
    notFound();
  }

  // If only one org or cookie already set and valid → redirect away from picker
  const activeSlug = await getActiveOrgSlug();
  const hasValidCookie = membershipsResult.organizations.some((o) => o.slug === activeSlug);
  if (membershipsResult.organizations.length === 1 || hasValidCookie) {
    const { redirect } = await import("next/navigation");
    redirect(`/${locale}/app`);
  }

  return (
    <main className={styles.main}>
      <h1>{t(locale, "selectOrganization.title")}</h1>
      <p>{t(locale, "selectOrganization.description")}</p>
      <OrgSelector locale={locale} organizations={membershipsResult.organizations} />
    </main>
  );
}
