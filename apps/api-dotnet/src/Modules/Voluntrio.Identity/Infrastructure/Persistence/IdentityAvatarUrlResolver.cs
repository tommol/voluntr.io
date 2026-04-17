using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;

namespace Voluntrio.Identity.Infrastructure.Persistence;

internal static class IdentityAvatarUrlResolver
{
    public static string? Resolve(string? email, string? pictureUrl)
    {
        if (!string.IsNullOrWhiteSpace(pictureUrl))
        {
            return NormalizeGravatarFallback(pictureUrl.Trim());
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(normalizedEmail));
        var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();

        return $"https://www.gravatar.com/avatar/{hash}?d=identicon";
    }

    private static string NormalizeGravatarFallback(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return url;
        }

        var host = uri.Host;
        if (!host.Equals("gravatar.com", StringComparison.OrdinalIgnoreCase)
            && !host.Equals("www.gravatar.com", StringComparison.OrdinalIgnoreCase)
            && !host.EndsWith(".gravatar.com", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        // Kinde can return Gravatar URLs with d=none; enforce identicon fallback.
        return Regex.Replace(
            url,
            @"([?&])d=none(?=(&|$))",
            "$1d=identicon",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}
