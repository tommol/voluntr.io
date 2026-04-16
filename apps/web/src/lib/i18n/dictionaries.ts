import en from "@/messages/en.json";
import pl from "@/messages/pl.json";
import { defaultLocale, type Locale } from "@/lib/i18n/config";

export type TranslationDictionary = typeof en;

const dictionaries: Record<Locale, TranslationDictionary> = {
  en,
  pl,
};

function getNestedValue(
  dictionary: TranslationDictionary,
  key: string,
): string | undefined {
  const value = key.split(".").reduce<unknown>((current, segment) => {
    if (!current || typeof current !== "object") {
      return undefined;
    }

    return (current as Record<string, unknown>)[segment];
  }, dictionary);

  return typeof value === "string" ? value : undefined;
}

export function getDictionary(locale: Locale): TranslationDictionary {
  return dictionaries[locale] ?? dictionaries[defaultLocale];
}

export function t(locale: Locale, key: string): string {
  const current = getDictionary(locale);
  const fallback = getDictionary(defaultLocale);

  return getNestedValue(current, key) ?? getNestedValue(fallback, key) ?? `[${key}]`;
}

