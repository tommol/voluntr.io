export const locales = ["en", "pl"] as const;

export type Locale = (typeof locales)[number];

export const defaultLocale: Locale = "en";
export const localeCookieName = "voluntr_locale";

export function isLocale(value: string): value is Locale {
  return locales.includes(value as Locale);
}

export function getLocaleFromPathname(pathname: string): Locale | null {
  const [, possibleLocale] = pathname.split("/");
  if (!possibleLocale || !isLocale(possibleLocale)) {
    return null;
  }

  return possibleLocale;
}

