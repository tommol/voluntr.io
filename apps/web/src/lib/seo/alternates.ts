import { locales, type Locale } from "@/lib/i18n/config";

export function buildAlternates(locale: Locale, pathSuffix = "") {
  const normalizedSuffix = pathSuffix.startsWith("/") ? pathSuffix : `/${pathSuffix}`;
  const suffix = normalizedSuffix === "/" ? "" : normalizedSuffix;

  const languages = Object.fromEntries(
    locales.map((availableLocale) => [availableLocale, `/${availableLocale}${suffix}`]),
  );

  return {
    canonical: `/${locale}${suffix}`,
    languages,
  };
}

