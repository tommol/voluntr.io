import { defaultLocale, isLocale, type Locale } from "@/lib/i18n/config";

export function getPreferredLocale(acceptLanguage: string | null): Locale {
  if (!acceptLanguage) {
    return defaultLocale;
  }

  const normalizedLanguages = acceptLanguage
    .split(",")
    .map((part) => part.trim().split(";")[0]?.toLowerCase())
    .filter(Boolean) as string[];

  for (const language of normalizedLanguages) {
    const [baseLanguage] = language.split("-");
    if (baseLanguage && isLocale(baseLanguage)) {
      return baseLanguage;
    }
  }

  return defaultLocale;
}

