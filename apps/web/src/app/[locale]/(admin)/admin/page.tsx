import type { Metadata } from "next";
import { t } from "@/lib/i18n/dictionaries";
import { isLocale } from "@/lib/i18n/config";
import { notFound } from "next/navigation";
import { buildAlternates } from "@/lib/seo/alternates";
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
    title: t(locale, "adminZone.title"),
    alternates: buildAlternates(locale, "/admin"),
  };
}

export default async function AdminZonePage({ params }: { params: LocaleParams }) {
  const { locale } = await params;
  if (!isLocale(locale)) {
    notFound();
  }

  return (
    <main className={styles.main}>
      <h1>{t(locale, "adminZone.title")}</h1>
      <p>{t(locale, "adminZone.description")}</p>
      <p className={styles.status}>{t(locale, "adminZone.status")}</p>
    </main>
  );
}
