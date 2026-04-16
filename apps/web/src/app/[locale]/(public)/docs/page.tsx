import type { Metadata } from "next";
import { t } from "@/lib/i18n/dictionaries";
import { isLocale } from "@/lib/i18n/config";
import { notFound } from "next/navigation";
import { buildAlternates } from "@/lib/seo/alternates";
import styles from "../../landing.module.css";

type LocaleParams = Promise<{ locale: string }>;

export async function generateMetadata({ params }: { params: LocaleParams }): Promise<Metadata> {
  const { locale } = await params;
  if (!isLocale(locale)) return {};
  return { title: t(locale, "docs.headline"), alternates: buildAlternates(locale, "/docs") };
}

export default async function DocsPage({ params }: { params: LocaleParams }) {
  const { locale } = await params;
  if (!isLocale(locale)) notFound();

  return (
    <main className={styles.main}>
      <h1>{t(locale, "docs.headline")}</h1>
      <p>{t(locale, "docs.description")}</p>
    </main>
  );
}
