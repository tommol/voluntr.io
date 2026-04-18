import type { Locale } from "@/lib/i18n/config";

export interface FeatureItem {
  iconKey: "boxes" | "folders" | "users" | "calendar" | "zap" | "rocket";
  title: string;
  description: string;
}

const items: Record<Locale, FeatureItem[]> = {
  en: [
    {
      iconKey: "boxes",
      title: "Everything in one place",
      description:
        "No more scattered files, spreadsheets and messages. The most important information is always at your fingertips.",
    },
    {
      iconKey: "folders",
      title: "Better team organisation",
      description: "Everyone knows what to do, and tasks and responsibilities are clearly assigned.",
    },
    {
      iconKey: "users",
      title: "Smoother member management",
      description: "Manage memberships, roles and fees without unnecessary administration.",
    },
    {
      iconKey: "calendar",
      title: "Easier event management",
      description: "Organise meetings, campaigns and initiatives faster and with greater control.",
    },
    {
      iconKey: "zap",
      title: "More time for your mission",
      description:
        "Less manual work and operational chaos means more energy for real organisational activities.",
    },
    {
      iconKey: "rocket",
      title: "Ready to grow",
      description:
        "Whether you run a small association or a larger foundation, Voluntrio grows with you.",
    },
  ],
  pl: [
    {
      iconKey: "boxes",
      title: "Wszystko w jednym miejscu",
      description:
        "Koniec z rozproszonymi plikami, arkuszami i wiadomościami. Najważniejsze informacje masz zawsze pod ręką.",
    },
    {
      iconKey: "folders",
      title: "Lepsza organizacja zespołu",
      description: "Każdy wie, co ma robić, a zadania i odpowiedzialności są jasno przypisane.",
    },
    {
      iconKey: "users",
      title: "Sprawniejsza obsługa członków",
      description: "Zarządzaj członkostwem, rolami i składkami bez zbędnej administracji.",
    },
    {
      iconKey: "calendar",
      title: "Łatwiejsze prowadzenie wydarzeń",
      description: "Organizuj spotkania, akcje i inicjatywy szybciej i z większą kontrolą.",
    },
    {
      iconKey: "zap",
      title: "Więcej czasu na misję",
      description:
        "Mniej ręcznej pracy i operacyjnego chaosu oznacza więcej energii na realne działania organizacji.",
    },
    {
      iconKey: "rocket",
      title: "Gotowe do rozwoju",
      description:
        "Niezależnie od tego, czy prowadzisz małe stowarzyszenie, czy większą fundację, Voluntrio rośnie razem z Wami.",
    },
  ],
};

export function getFeatureItems(locale: Locale): FeatureItem[] {
  return items[locale] ?? items.en;
}
