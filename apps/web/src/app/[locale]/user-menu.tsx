import type { Locale } from "@/lib/i18n/config";
import styles from "./user-menu.module.css";

interface UserMenuProps {
  locale: Locale;
  userName?: string;
  userAvatarUrl?: string;
}

function getInitials(name: string): string {
  return name
    .split(" ")
    .filter((part) => part.length > 0)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("") || "U";
}

export function UserMenu({ locale, userName, userAvatarUrl }: UserMenuProps) {
  const displayName = userName?.trim() || "User";
  const initials = getInitials(displayName);
  const profileLabel = locale === "pl" ? "Profil" : "Profile";
  const logoutLabel = locale === "pl" ? "Wyloguj" : "Logout";
  const logoutRedirectPath = `/${locale}/signed-out`;
  const logoutUrl = `/api/auth/logout?post_logout_redirect_url=${encodeURIComponent(logoutRedirectPath)}`;
  const avatarStyle = userAvatarUrl ? { backgroundImage: `url(${userAvatarUrl})` } : undefined;

  return (
    <details className={styles.menu}>
      <summary className={styles.trigger}>
        <span className={styles.avatar} style={avatarStyle} aria-hidden="true">
          {!userAvatarUrl && <span className={styles.initials}>{initials}</span>}
        </span>
        <span className={styles.name}>{displayName}</span>
        <span className={styles.caret} aria-hidden="true">▾</span>
      </summary>

      <div className={styles.panel}>
        <a href={`/${locale}/profile`} className={styles.item}>
          {profileLabel}
        </a>
        <a href={logoutUrl} className={styles.item}>
          {logoutLabel}
        </a>
      </div>
    </details>
  );
}
