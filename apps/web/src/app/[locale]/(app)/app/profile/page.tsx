import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { buildAlternates } from "@/lib/seo/alternates";
import { isLocale } from "@/lib/i18n/config";
import { t } from "@/lib/i18n/dictionaries";
import { requireAuth } from "@/lib/auth/guards";
import { getMyIdentityProfile } from "@/lib/api/identity";
import { getMyOrganizations } from "@/lib/api/organizations";
import { getActiveOrgSlug, resolveActiveOrg } from "@/lib/auth/active-org";
import styles from "./profile.module.css";

type LocaleParams = Promise<{ locale: string }>;

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
    title: t(locale, "profile.title"),
    alternates: buildAlternates(locale, "/app/profile"),
  };
}

export default async function ProfilePage({ params }: { params: LocaleParams }) {
  const { locale } = await params;
  if (!isLocale(locale)) {
    notFound();
  }

  const session = await requireAuth(locale, `/${locale}/app/profile`);
  const profile = session.accessToken
    ? await getMyIdentityProfile(session.accessToken).catch(() => null)
    : null;
  const organizationsResult = session.accessToken
    ? await getMyOrganizations(session.accessToken).catch(() => ({ organizations: [], hasMembership: false, requiresReauth: false }))
    : { organizations: [], hasMembership: false, requiresReauth: false };
  const activeOrgSlug = await getActiveOrgSlug();
  const activeOrganization = resolveActiveOrg(organizationsResult.organizations, activeOrgSlug);

  const email = profile?.email ?? t(locale, "profile.missingValue");
  const displayName = [profile?.givenName, profile?.familyName].filter(Boolean).join(" ")
    || profile?.email
    || session.userName
    || t(locale, "profile.missingValue");
  const avatarUrl = profile?.avatarUrl ?? session.userAvatarUrl;
  const initials = displayName
    .split(" ")
    .filter((part) => part.length > 0)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("") || "U";

  return (
    <main className={styles.main}>
      <header className={styles.header}>
        <h1>{t(locale, "profile.title")}</h1>
      </header>

      <section className={styles.card}>
        <div className={styles.avatarRow}>
          <span
            className={styles.avatar}
            style={avatarUrl ? { backgroundImage: `url(${avatarUrl})` } : undefined}
            aria-hidden="true"
          >
            {!avatarUrl && <span className={styles.initials}>{initials}</span>}
          </span>
          <div>
            <p className={styles.displayName}>{displayName}</p>
            <p className={styles.subtle}>{email}</p>
          </div>
        </div>

        <dl className={styles.list}>
          <div className={styles.row}>
            <dt>{t(locale, "profile.nameLabel")}</dt>
            <dd>{displayName}</dd>
          </div>

          <div className={styles.row}>
            <dt>{t(locale, "profile.emailLabel")}</dt>
            <dd>{email}</dd>
          </div>

          <div className={styles.row}>
            <dt>{t(locale, "profile.organizationsLabel")}</dt>
            <dd>
              {organizationsResult.organizations.length > 0 ? (
                <ul className={styles.orgList}>
                  {organizationsResult.organizations.map((organization) => {
                    const isActive = activeOrganization?.slug === organization.slug;
                    return (
                      <li key={organization.organizationId} className={styles.orgItem}>
                        <div className={styles.orgContent}>
                          <span className={styles.orgName}>{organization.name}</span>
                          <span className={styles.orgRole}>
                            {t(locale, "profile.organizationRoleLabel")}: {organization.role}
                          </span>
                        </div>
                        <div className={styles.orgFlags}>
                          {isActive && (
                            <span className={styles.activeBadge}>{t(locale, "profile.activeOrganization")}</span>
                          )}
                        </div>
                      </li>
                    );
                  })}
                </ul>
              ) : (
                t(locale, "profile.organizationsEmpty")
              )}
            </dd>
          </div>
        </dl>
      </section>
    </main>
  );
}
