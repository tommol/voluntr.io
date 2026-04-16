import { NextRequest, NextResponse } from "next/server";
import { getLocaleFromPathname, isLocale, localeCookieName, type Locale } from "@/lib/i18n/config";
import { getPreferredLocale } from "@/lib/i18n/locale-detection";

const ONE_YEAR_IN_SECONDS = 60 * 60 * 24 * 365;

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

export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const localeFromPath = getLocaleFromPathname(pathname);

  if (!localeFromPath) {
    const targetLocale = resolveLocaleFromRequest(request);
    const localizedPath = pathname === "/" ? `/${targetLocale}` : `/${targetLocale}${pathname}`;
    return buildLocalizedRedirect(request, targetLocale, localizedPath);
  }

  return appendLocaleCookie(NextResponse.next(), localeFromPath);
}

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico|.*\\..*).*)"],
};
