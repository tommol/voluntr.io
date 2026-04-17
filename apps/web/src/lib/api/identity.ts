export interface IdentityProfileResponse {
  givenName?: string;
  familyName?: string;
  email?: string;
  avatarUrl?: string;
}

const apiBaseUrl = (process.env.VOLUNTRIO_API_URL ?? "http://localhost:5177").replace(/\/$/, "");

export async function getMyIdentityProfile(accessToken: string): Promise<IdentityProfileResponse | null> {
  const response = await fetch(`${apiBaseUrl}/api/v1/identity/me`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
    cache: "no-store",
  });

  if (response.status === 401 || response.status === 403 || response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error(`Failed to load identity profile: ${response.status}`);
  }

  return (await response.json()) as IdentityProfileResponse;
}
