import { requireAuth } from "@/lib/auth/guards";
import { isLocale } from "@/lib/i18n/config";
import { notFound } from "next/navigation";
import { SiteHeader } from "../../site-header";
import shellStyles from "../../zone-layout.module.css";
import { LanguageSwitcher } from "../../language-switcher";
import { UserMenu } from "../../user-menu";

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

  return (
    <div className={shellStyles.shell}>
      <SiteHeader
        locale={locale}
        meta={
          <>
            <LanguageSwitcher locale={locale} />
            <UserMenu locale={locale} userName={session.userName} userAvatarUrl={session.userAvatarUrl} />
          </>
        }
      />
      {children}
    </div>
  );
}
