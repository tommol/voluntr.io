import type { Metadata } from "next";
import { t } from "@/lib/i18n/dictionaries";
import { isLocale } from "@/lib/i18n/config";
import { notFound } from "next/navigation";
import { buildAlternates } from "@/lib/seo/alternates";
import { requireAuth, requireOrganizationMembership } from "@/lib/auth/guards";
import styles from "../../landing.module.css";

type LocaleParams = Promise<{ locale: string }>;

export async function generateMetadata({
  params,
}: {
  params: LocaleParams;
}): Promise<Metadata> {
  const { locale } = await params;
  if (!isLocale(locale)) {
    return {};
  }

  return {
    title: t(locale, "appZone.title"),
    alternates: buildAlternates(locale, "/app"),
  };
}

export default async function AppZonePage({ params }: { params: LocaleParams }) {
  const { locale } = await params;
  if (!isLocale(locale)) {
    notFound();
  }

  const session = await requireAuth(locale, `/${locale}/app`);
  await requireOrganizationMembership(locale, session);

  return (
    <main className={styles.main}>
      <h1>{t(locale, "appZone.title")}</h1>
      <p>{t(locale, "appZone.description")}</p>
      <p className={styles.status}>{t(locale, "appZone.status")}</p>
    </main>
  );
}
