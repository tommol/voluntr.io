import { t } from "@/lib/i18n/dictionaries";
import { isLocale } from "@/lib/i18n/config";
import { notFound } from "next/navigation";
import styles from "../zone-layout.module.css";

type LocaleParams = Promise<{ locale: string }>;

export default async function PublicLayout({
  children,
  params,
}: Readonly<{
  children: React.ReactNode;
  params: LocaleParams;
}>) {
  const { locale } = await params;
  if (!isLocale(locale)) {
    notFound();
  }

  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <nav className={styles.nav}>
          <a href={`/${locale}`}>{t(locale, "common.nav.landing")}</a>
          <a href={`/${locale}/app`}>{t(locale, "common.nav.app")}</a>
          <a href={`/${locale}/admin`}>{t(locale, "common.nav.admin")}</a>
        </nav>
        <p className={styles.meta}>
          {t(locale, "common.languageLabel")}: {locale.toUpperCase()}
        </p>
      </header>
      {children}
    </div>
  );
}

