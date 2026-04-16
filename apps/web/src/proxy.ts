import { NextRequest, NextResponse } from "next/server";
import { getSessionFromCookies } from "@/lib/auth/session";
import { getLocaleFromPathname, isLocale, localeCookieName, type Locale } from "@/lib/i18n/config";
import { getPreferredLocale } from "@/lib/i18n/locale-detection";

const ONE_YEAR_IN_SECONDS = 60 * 60 * 24 * 365;
const truthyEnvValues = new Set(["1", "true", "yes", "on"]);
const ALLOW_ZONE_ACCESS_WITHOUT_AUTH = truthyEnvValues.has(
  (process.env.ALLOW_ZONE_ACCESS_WITHOUT_AUTH ?? "").trim().toLowerCase(),
);

function appendLocaleCookie(response: NextResponse, locale: Locale) {
  response.cookies.set(localeCookieName, locale, {
    path: "/",
    sameSite: "lax",
    maxAge: ONE_YEAR_IN_SECONDS,
  });

  return response;
}

function buildLocalizedRedirect(request: NextRequest, locale: Locale, pathname: string) {
  const url = request.nextUrl.clone();
  url.pathname = pathname;

  return appendLocaleCookie(NextResponse.redirect(url), locale);
}

function resolveLocaleFromRequest(request: NextRequest): Locale {
  const localeFromCookie = request.cookies.get(localeCookieName)?.value;
  if (localeFromCookie && isLocale(localeFromCookie)) {
    return localeFromCookie;
  }

  return getPreferredLocale(request.headers.get("accept-language"));
}

function isPathInZone(pathname: string, locale: Locale, zone: "app" | "admin"): boolean {
  const prefix = `/${locale}/${zone}`;
  return pathname === prefix || pathname.startsWith(`${prefix}/`);
}

export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const localeFromPath = getLocaleFromPathname(pathname);

  if (!localeFromPath) {
    const targetLocale = resolveLocaleFromRequest(request);
    const localizedPath = pathname === "/" ? `/${targetLocale}` : `/${targetLocale}${pathname}`;
    return buildLocalizedRedirect(request, targetLocale, localizedPath);
  }

  if (!ALLOW_ZONE_ACCESS_WITHOUT_AUTH) {
    const session = getSessionFromCookies(request.cookies);

    if (
      (isPathInZone(pathname, localeFromPath, "app") || isPathInZone(pathname, localeFromPath, "admin")) &&
      !session?.isAuthenticated
    ) {
      return buildLocalizedRedirect(request, localeFromPath, `/${localeFromPath}?auth=required`);
    }

    if (isPathInZone(pathname, localeFromPath, "admin") && !session?.roles.includes("admin")) {
      return buildLocalizedRedirect(request, localeFromPath, `/${localeFromPath}/app?forbidden=1`);
    }
  }

  return appendLocaleCookie(NextResponse.next(), localeFromPath);
}

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico|.*\\..*).*)"],
};

