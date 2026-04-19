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
    title: t(locale, "signedOut.title"),
    alternates: buildAlternates(locale, "/signed-out"),
  };
}

export default async function SignedOutPage({ params }: { params: LocaleParams }) {
  const { locale } = await params;
  if (!isLocale(locale)) {
    notFound();
  }

  return (
    <main className={styles.main}>
      <h1>{t(locale, "signedOut.title")}</h1>
      <p>{t(locale, "signedOut.description")}</p>
      <p className={styles.status}>{t(locale, "signedOut.hint")}</p>
      <p>
        <a href={`/${locale}`}>
          {t(locale, "signedOut.backToHome")}
        </a>
      </p>
    </main>
  );
}
