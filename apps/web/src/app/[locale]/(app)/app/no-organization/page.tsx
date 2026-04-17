import type { Metadata } from "next";
import { t } from "@/lib/i18n/dictionaries";
import { isLocale } from "@/lib/i18n/config";
import { notFound } from "next/navigation";
import { buildAlternates } from "@/lib/seo/alternates";
import styles from "../../../landing.module.css";

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
    title: t(locale, "noOrganization.title"),
    alternates: buildAlternates(locale, "/app/no-organization"),
  };
}

export default async function NoOrganizationPage({ params }: { params: LocaleParams }) {
  const { locale } = await params;
  if (!isLocale(locale)) {
    notFound();
  }

  return (
    <main className={styles.main}>
      <h1>{t(locale, "noOrganization.title")}</h1>
      <p>{t(locale, "noOrganization.description")}</p>
      <p className={styles.status}>{t(locale, "noOrganization.hint")}</p>
    </main>
  );
}
