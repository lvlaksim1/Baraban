using Baraban.Models;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace Baraban.Services;

public sealed class WebViewSessionBridge(SessionStore store)
{
    private readonly HashSet<string> _excludedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cookie", "Host", "Content-Length", "Connection"
    };

    public async Task InitializeAsync(WebView2 webView, SessionProfile session, Action<string>? observedHeader = null)
    {
        var environment = await CoreWebView2Environment.CreateAsync(null, store.WebViewDataDirectory);
        await webView.EnsureCoreWebView2Async(environment);
        webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
        webView.CoreWebView2.WebResourceRequested += (_, args) =>
        {
            try
            {
                foreach (var header in args.Request.Headers)
                {
                    if (_excludedHeaders.Contains(header.Key))
                        continue;
                    if (!ShouldCapture(header.Key))
                        continue;
                    session.Headers[header.Key] = header.Value;
                    observedHeader?.Invoke(header.Key);
                }
            }
            catch
            {
            }
        };

        await RestoreCookiesToBrowserAsync(webView, session);
    }

    public async Task SyncCookiesFromBrowserAsync(WebView2 webView, SessionProfile session, string? url = null)
    {
        if (webView.CoreWebView2 is null)
            return;

        var cookies = await webView.CoreWebView2.CookieManager.GetCookiesAsync(url ?? "");
        session.Cookies = cookies.Select(cookie => new StoredCookie
        {
            Name = cookie.Name,
            Value = cookie.Value,
            Domain = cookie.Domain,
            Path = cookie.Path,
            ExpiresUtc = cookie.IsSession
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(cookie.Expires, DateTimeKind.Utc)),
            IsHttpOnly = cookie.IsHttpOnly,
            IsSecure = cookie.IsSecure,
            SameSite = cookie.SameSite.ToString()
        }).ToList();
        store.Save(session);
    }

    public async Task RestoreCookiesToBrowserAsync(WebView2 webView, SessionProfile session)
    {
        if (webView.CoreWebView2 is null)
            return;

        var manager = webView.CoreWebView2.CookieManager;
        var now = DateTimeOffset.UtcNow;
        foreach (var stored in session.Cookies.Where(c => !c.IsExpired(now)))
        {
            if (string.IsNullOrWhiteSpace(stored.Name) || string.IsNullOrWhiteSpace(stored.Domain))
                continue;

            var cookie = manager.CreateCookie(stored.Name, stored.Value, stored.Domain, string.IsNullOrWhiteSpace(stored.Path) ? "/" : stored.Path);
            cookie.IsHttpOnly = stored.IsHttpOnly;
            cookie.IsSecure = stored.IsSecure;
            if (stored.ExpiresUtc is { } expires)
                cookie.Expires = expires.UtcDateTime;
            if (Enum.TryParse<CoreWebView2CookieSameSiteKind>(stored.SameSite, true, out var sameSite))
                cookie.SameSite = sameSite;
            manager.AddOrUpdateCookie(cookie);
        }

        await Task.CompletedTask;
    }

    private static bool ShouldCapture(string header) =>
        header.Equals("X-CSRF-Token", StringComparison.OrdinalIgnoreCase)
        || header.StartsWith("X-GIB-", StringComparison.OrdinalIgnoreCase)
        || header.Equals("Origin", StringComparison.OrdinalIgnoreCase)
        || header.Equals("Referer", StringComparison.OrdinalIgnoreCase)
        || header.Equals("Accept-Language", StringComparison.OrdinalIgnoreCase)
        || header.Equals("User-Agent", StringComparison.OrdinalIgnoreCase)
        || header.StartsWith("sec-ch-", StringComparison.OrdinalIgnoreCase);
}
