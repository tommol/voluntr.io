import type { Metadata } from "next";
import { t } from "@/lib/i18n/dictionaries";
import { isLocale } from "@/lib/i18n/config";
import { notFound } from "next/navigation";
import { buildAlternates } from "@/lib/seo/alternates";
import styles from "../landing.module.css";

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
    title: t(locale, "landing.headline"),
    alternates: buildAlternates(locale),
  };
}

export default async function LandingPage({
  params,
  searchParams,
}: {
  params: LocaleParams;
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const { locale } = await params;
  if (!isLocale(locale)) {
    notFound();
  }

  const query = await searchParams;
  const authRequired = query.auth === "required";
  const forbidden = query.forbidden === "1";

  return (
    <main className={styles.main}>
      <h1>{t(locale, "landing.headline")}</h1>
      <p>{t(locale, "landing.description")}</p>
      {authRequired ? <p className={styles.warning}>{t(locale, "auth.requiredHint")}</p> : null}
      {forbidden ? <p className={styles.warning}>{t(locale, "auth.forbiddenHint")}</p> : null}
      <div className={styles.actions}>
        <a className={styles.primary} href={`/${locale}/app`}>
          {t(locale, "landing.ctaPrimary")}
        </a>
        <a className={styles.secondary} href={`/${locale}/admin`}>
          {t(locale, "landing.ctaSecondary")}
        </a>
      </div>
    </main>
  );
}
