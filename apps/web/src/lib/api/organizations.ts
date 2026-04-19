export interface OrganizationMembership {
  organizationId: string;
  name: string;
  slug: string;
  role: string;
}

interface MembershipResponse {
  organizations: OrganizationMembership[];
}

export interface MembershipCheckResult {
  organizations: OrganizationMembership[];
  hasMembership: boolean;
  requiresReauth: boolean;
}

const apiBaseUrl = (process.env.VOLUNTRIO_API_URL ?? "http://localhost:5177").replace(/\/$/, "");

export async function getMyOrganizations(accessToken: string): Promise<MembershipCheckResult> {
  let response: Response;
  try {
    response = await fetch(`${apiBaseUrl}/api/v1/organizations/me/organizations`, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
      cache: "no-store",
    });
  } catch {
    // Treat API connectivity issues as no membership to avoid crashing app layout.
    return { organizations: [], hasMembership: false, requiresReauth: false };
  }

  if (response.status === 401) {
    return { organizations: [], hasMembership: false, requiresReauth: true };
  }

  if (response.status === 403 || response.status === 404) {
    return { organizations: [], hasMembership: false, requiresReauth: false };
  }

  if (!response.ok) {
    throw new Error(`Failed to load organizations: ${response.status}`);
  }

  const body = (await response.json()) as MembershipResponse;
  return {
    organizations: body.organizations,
    hasMembership: body.organizations.length > 0,
    requiresReauth: false,
  };
}
