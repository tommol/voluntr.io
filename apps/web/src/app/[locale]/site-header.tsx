import Image from "next/image";
import styles from "./site-header.module.css";
import type { Locale } from "@/lib/i18n/config";

interface SiteHeaderProps {
  locale: Locale;
  nav?: React.ReactNode;
  meta?: React.ReactNode;
}

export function SiteHeader({ locale, nav, meta }: SiteHeaderProps) {
  return (
    <header className={styles.header}>
      <a href={`/${locale}`} className={styles.logoLink}>
        <Image src="/logo.png" alt="voluntr" height={48} width={48} />
        <span className={styles.brand}>
          voluntr<span className={styles.brandSuffix}>.io</span>
        </span>
      </a>
      {nav && <nav className={styles.nav}>{nav}</nav>}
      {meta && <div className={styles.meta}>{meta}</div>}
    </header>
  );
}
