interface MembershipResponse {
  organizations: Array<{
    organizationId: string;
    name: string;
    slug: string;
    role: string;
  }>;
}

const apiBaseUrl = (process.env.VOLUNTRIO_API_URL ?? "http://localhost:5100").replace(/\/$/, "");

export async function getMyOrganizations(accessToken: string): Promise<{ hasMembership: boolean }> {
  const response = await fetch(`${apiBaseUrl}/api/v1/organizations/me/organizations`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
    cache: "no-store",
  });

  if (response.status === 403 || response.status === 404) {
    return { hasMembership: false };
  }

  if (!response.ok) {
    throw new Error(`Failed to load organizations: ${response.status}`);
  }

  const body = (await response.json()) as MembershipResponse;
  return { hasMembership: body.organizations.length > 0 };
}
