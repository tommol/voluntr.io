import type { Metadata } from "next";
import { t } from "@/lib/i18n/dictionaries";
import { isLocale } from "@/lib/i18n/config";
import { notFound } from "next/navigation";
import { buildAlternates } from "@/lib/seo/alternates";
import { Boxes, CalendarDays, FolderKanban, Handshake, Rocket, Users, Zap, type LucideIcon } from "lucide-react";
import { getFeatureItems } from "./features-data";
import heroStyles from "./hero.module.css";
import featuresStyles from "./features.module.css";

type LocaleParams = Promise<{ locale: string }>;

const featureIconMap: Record<string, LucideIcon> = {
  boxes: Boxes,
  folders: FolderKanban,
  users: Users,
  calendar: CalendarDays,
  zap: Zap,
  rocket: Rocket,
};

export async function generateMetadata({
  params,
}: {
  params: LocaleParams;
}): Promise<Metadata> {
  const { locale } = await params;
  if (!isLocale(locale)) {
    return {};
  }

  return {
    title: t(locale, "landing.headline"),
    alternates: buildAlternates(locale),
  };
}

export default async function LandingPage({
  params,
  searchParams,
}: {
  params: LocaleParams;
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const { locale } = await params;
  if (!isLocale(locale)) {
    notFound();
  }

  const query = await searchParams;
  const authRequired = query.auth === "required";
  const forbidden = query.forbidden === "1";

  return (
    <>
      <section className={heroStyles.hero}>
        <div className={heroStyles.heroContent}>
          <span className={heroStyles.eyebrow}>{t(locale, "landing.eyebrow")}</span>
          <h1 className={heroStyles.headline}>
            {t(locale, "landing.headline")}{" "}
            <span className={heroStyles.headlineAccent}>{t(locale, "landing.headlineAccent")}</span>
          </h1>
          <p className={heroStyles.description}>{t(locale, "landing.description")}</p>
          <div className={heroStyles.notices}>
            {authRequired ? <p className={heroStyles.warning}>{t(locale, "auth.requiredHint")}</p> : null}
            {forbidden ? <p className={heroStyles.warning}>{t(locale, "auth.forbiddenHint")}</p> : null}
          </div>
          <div className={heroStyles.actions}>
            <a className={heroStyles.primary} href={`/${locale}/app`}>
              {t(locale, "landing.ctaPrimary")}
            </a>
            <a className={heroStyles.secondary} href={`/${locale}/admin`}>
              {t(locale, "landing.ctaSecondary")}
            </a>
          </div>
        </div>
        <div className={heroStyles.heroVisual}>
          <div className={heroStyles.visualCard}>
            <Handshake className={heroStyles.visualIcon} aria-hidden="true" />
          </div>
        </div>
      </section>
      <section className={featuresStyles.section}>
        <div className={featuresStyles.inner}>
          <h2 className={featuresStyles.headline}>{t(locale, "features.headline")}</h2>
          <div className={featuresStyles.grid}>
            {getFeatureItems(locale).map((item) => {
              const FeatureIcon = featureIconMap[item.iconKey] ?? Boxes;
              return (
                <div key={item.title} className={featuresStyles.card}>
                  <div className={featuresStyles.titleRow}>
                    <span className={featuresStyles.icon} aria-hidden="true">
                      <FeatureIcon />
                    </span>
                    <h3 className={featuresStyles.title}>{item.title}</h3>
                  </div>
                  <p className={featuresStyles.description}>{item.description}</p>
                </div>
              );
            })}
          </div>
        </div>
      </section>
    </>
  );
}
