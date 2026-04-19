import { NextRequest, NextResponse } from "next/server";
import { ACTIVE_ORG_COOKIE } from "@/lib/auth/active-org";

const ONE_DAY = 60 * 60 * 24;

export async function POST(request: NextRequest) {
  const body = await request.json().catch(() => null);
  const slug = typeof body?.slug === "string" ? body.slug.trim() : null;

  if (!slug) {
    return NextResponse.json({ error: "slug is required" }, { status: 400 });
  }

  const redirectTo = typeof body?.redirectTo === "string" ? body.redirectTo : null;

  const response = redirectTo
    ? NextResponse.redirect(new URL(redirectTo, request.url))
    : NextResponse.json({ ok: true });

  response.cookies.set(ACTIVE_ORG_COOKIE, slug, {
    path: "/",
    sameSite: "lax",
    maxAge: ONE_DAY,
    httpOnly: true,
  });

  return response;
}
