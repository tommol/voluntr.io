"use client";

import { usePathname } from "next/navigation";
import styles from "./language-switcher.module.css";
import { locales, type Locale } from "@/lib/i18n/config";

interface LanguageSwitcherProps {
  locale: Locale;
}

export function LanguageSwitcher({ locale }: LanguageSwitcherProps) {
  const pathname = usePathname();

  // Strip current locale prefix and replace with target locale
  const pathWithoutLocale = pathname.replace(new RegExp(`^/${locale}`), "") || "/";

  return (
    <div className={styles.switcher}>
      {locales.map((lng, i) => (
        <span key={lng} className={styles.item}>
          {i > 0 && <span className={styles.sep}>|</span>}
          <a
            href={`/${lng}${pathWithoutLocale === "/" ? "" : pathWithoutLocale}`}
            className={lng === locale ? styles.active : styles.link}
          >
            {lng.toUpperCase()}
          </a>
        </span>
      ))}
    </div>
  );
}
