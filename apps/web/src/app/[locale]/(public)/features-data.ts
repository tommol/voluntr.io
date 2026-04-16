import type { Locale } from "@/lib/i18n/config";

export interface FeatureItem {
  icon: string;
  title: string;
  description: string;
}

const items: Record<Locale, FeatureItem[]> = {
  en: [
    {
      icon: "📦",
      title: "Everything in one place",
      description:
        "No more scattered files, spreadsheets and messages. The most important information is always at your fingertips.",
    },
    {
      icon: "🗂️",
      title: "Better team organisation",
      description: "Everyone knows what to do, and tasks and responsibilities are clearly assigned.",
    },
    {
      icon: "👥",
      title: "Smoother member management",
      description: "Manage memberships, roles and fees without unnecessary administration.",
    },
    {
      icon: "📅",
      title: "Easier event management",
      description: "Organise meetings, campaigns and initiatives faster and with greater control.",
    },
    {
      icon: "⚡",
      title: "More time for your mission",
      description:
        "Less manual work and operational chaos means more energy for real organisational activities.",
    },
    {
      icon: "🚀",
      title: "Ready to grow",
      description:
        "Whether you run a small association or a larger foundation, Voluntrio grows with you.",
    },
  ],
  pl: [
    {
      icon: "📦",
      title: "Wszystko w jednym miejscu",
      description:
        "Koniec z rozproszonymi plikami, arkuszami i wiadomościami. Najważniejsze informacje masz zawsze pod ręką.",
    },
    {
      icon: "🗂️",
      title: "Lepsza organizacja zespołu",
      description: "Każdy wie, co ma robić, a zadania i odpowiedzialności są jasno przypisane.",
    },
    {
      icon: "👥",
      title: "Sprawniejsza obsługa członków",
      description: "Zarządzaj członkostwem, rolami i składkami bez zbędnej administracji.",
    },
    {
      icon: "📅",
      title: "Łatwiejsze prowadzenie wydarzeń",
      description: "Organizuj spotkania, akcje i inicjatywy szybciej i z większą kontrolą.",
    },
    {
      icon: "⚡",
      title: "Więcej czasu na misję",
      description:
        "Mniej ręcznej pracy i operacyjnego chaosu oznacza więcej energii na realne działania organizacji.",
    },
    {
      icon: "🚀",
      title: "Gotowe do rozwoju",
      description:
        "Niezależnie od tego, czy prowadzisz małe stowarzyszenie, czy większą fundację, Voluntrio rośnie razem z Wami.",
    },
  ],
};

export function getFeatureItems(locale: Locale): FeatureItem[] {
  return items[locale] ?? items.en;
}
