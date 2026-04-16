import { requireAuth, requireOrganizationMembership } from "@/lib/auth/guards";
import { isLocale } from "@/lib/i18n/config";
import { notFound } from "next/navigation";
import { SiteHeader } from "../../site-header";
import shellStyles from "../../zone-layout.module.css";
import { LanguageSwitcher } from "../../language-switcher";

type LocaleParams = Promise<{ locale: string }>;

export default async function AppZoneLayout({
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

  const session = await requireAuth(locale, `/${locale}/app`);
  await requireOrganizationMembership(locale, session);

  return (
    <div className={shellStyles.shell}>
      <SiteHeader locale={locale} meta={<><LanguageSwitcher locale={locale} />{session.userId ?? "user"}</>} />
      {children}
    </div>
  );
}
