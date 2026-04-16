import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { getDictionary, t } from "@/lib/i18n/dictionaries";
import { isLocale, locales, type Locale } from "@/lib/i18n/config";
import "../globals.css";

type LocaleParams = Promise<{ locale: string }>;

export function generateStaticParams() {
  return locales.map((locale) => ({ locale }));
}

export async function generateMetadata({
  params,
}: {
  params: LocaleParams;
}): Promise<Metadata> {
  const { locale } = await params;
  const currentLocale: Locale = isLocale(locale) ? locale : "en";
  const dictionary = getDictionary(currentLocale);

  return {
    title: {
      default: dictionary.seo.defaultTitle,
      template: `%s | ${dictionary.seo.titleTemplateSuffix}`,
    },
    description: dictionary.seo.description,
    alternates: {
      canonical: `/${currentLocale}`,
      languages: {
        en: "/en",
        pl: "/pl",
      },
    },
  };
}

export default async function LocaleLayout({
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
    <html lang={locale}>
      <body>
        {children}
      </body>
    </html>
  );
}
