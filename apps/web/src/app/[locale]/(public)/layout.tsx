import { t } from "@/lib/i18n/dictionaries";
import { isLocale } from "@/lib/i18n/config";
import { notFound } from "next/navigation";
import { SiteHeader } from "../site-header";
import { LanguageSwitcher } from "../language-switcher";
import shellStyles from "../zone-layout.module.css";

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
    <div className={shellStyles.shell}>
      <SiteHeader
        locale={locale}
        nav={
          <>
            <a href={`/${locale}/pricing`}>{t(locale, "common.nav.pricing")}</a>
            <a href={`/${locale}/docs`}>{t(locale, "common.nav.docs")}</a>
            <a href={`/${locale}/about`}>{t(locale, "common.nav.about")}</a>
          </>
        }
        meta={<LanguageSwitcher locale={locale} />}
      />
      {children}
    </div>
  );
}

