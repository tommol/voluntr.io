import { getKindeServerSession } from "@kinde-oss/kinde-auth-nextjs/server";
import { getMyIdentityProfile } from "@/lib/api/identity";

export interface Session {
  isAuthenticated: boolean;
  userId?: string;
  userName?: string;
  userAvatarUrl?: string;
  roles: string[];
  accessToken?: string;
}

export async function getSession(): Promise<Session | null> {
  const { isAuthenticated, getUser, getRoles, getAccessTokenRaw, getIdTokenRaw } = getKindeServerSession();
  const [authenticated, accessToken, idToken] = await Promise.all([
    isAuthenticated(),
    getAccessTokenRaw(),
    getIdTokenRaw(),
  ]);

  // In some callback edge-cases isAuthenticated can be temporarily false
  // while token cookies are already present.
  if (!authenticated && !accessToken && !idToken) {
    return null;
  }

  const user = await getUser().catch(() => null);
  const rolesResult = await getRoles().catch(() => null);
  const profile = accessToken ? await getMyIdentityProfile(accessToken).catch(() => null) : null;

  const userAvatarUrl = profile?.avatarUrl ?? undefined;
  const givenName = profile?.givenName ?? user?.given_name;
  const familyName = profile?.familyName ?? user?.family_name;
  const displayName = [givenName, familyName].filter(Boolean).join(" ") || profile?.email || user?.email || undefined;

  return {
    isAuthenticated: true,
    userId: user?.id,
    userName: displayName,
    userAvatarUrl,
    roles: rolesResult?.map((role) => role.key) ?? [],
    accessToken: accessToken ?? undefined,
  };
}
