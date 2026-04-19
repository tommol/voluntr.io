"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import type { OrganizationMembership } from "@/lib/auth/guards";
import styles from "./select-organization.module.css";

interface Props {
  locale: string;
  organizations: OrganizationMembership[];
}

export function OrgSelector({ locale, organizations }: Props) {
  const router = useRouter();
  const [pending, setPending] = useState<string | null>(null);

  async function select(slug: string) {
    setPending(slug);
    await fetch("/api/org", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ slug, redirectTo: `/${locale}/app` }),
    });
    router.replace(`/${locale}/app`);
    router.refresh();
  }

  return (
    <ul className={styles.list}>
      {organizations.map((org) => (
        <li key={org.slug}>
          <button
            className={styles.card}
            onClick={() => select(org.slug)}
            disabled={pending !== null}
            aria-busy={pending === org.slug}
          >
            <span className={styles.orgName}>{org.name}</span>
            <span className={styles.orgRole}>{org.role}</span>
            {pending === org.slug && <span className={styles.spinner} aria-hidden="true" />}
          </button>
        </li>
      ))}
    </ul>
  );
}
