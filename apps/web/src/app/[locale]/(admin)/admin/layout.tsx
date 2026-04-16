import { requireRole } from "@/lib/auth/guards";
import { isLocale } from "@/lib/i18n/config";
import { notFound } from "next/navigation";
import { SiteHeader } from "../../site-header";
import shellStyles from "../../zone-layout.module.css";
import { LanguageSwitcher } from "../../language-switcher";

type LocaleParams = Promise<{ locale: string }>;

export default async function AdminZoneLayout({
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

  const session = await requireRole(locale, "admin");

  return (
    <div className={shellStyles.shell}>
      <SiteHeader
        locale={locale}
        meta={<><LanguageSwitcher locale={locale} />{session.userId ?? "admin"} · {session.roles.join(", ")}</>}
      />
      {children}
    </div>
  );
}

