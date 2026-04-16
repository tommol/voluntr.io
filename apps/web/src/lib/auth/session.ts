import { getKindeServerSession } from "@kinde-oss/kinde-auth-nextjs/server";

export interface Session {
  isAuthenticated: boolean;
  userId?: string;
  roles: string[];
  accessToken?: string;
}

export async function getSession(): Promise<Session | null> {
  const { isAuthenticated, getUser, getRoles, getAccessTokenRaw } = getKindeServerSession();
  const authenticated = await isAuthenticated();

  if (!authenticated) {
    return null;
  }

  const user = await getUser();
  const rolesResult = await getRoles();
  const accessToken = await getAccessTokenRaw();

  return {
    isAuthenticated: true,
    userId: user?.id,
    roles: rolesResult?.map((role) => role.key) ?? [],
    accessToken: accessToken ?? undefined,
  };
}
