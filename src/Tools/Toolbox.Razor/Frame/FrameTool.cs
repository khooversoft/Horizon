using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Razor.Frame;

public static class FrameTool
{
    public static readonly string DarkModeCookieName = "dark-mode";
    public static readonly string DarkModeCookiePath = "/preferences/dark-mode";
    public static readonly string ThemeNameCookieName = "theme-name";
    public static readonly string ThemeNameCookiePath = "/preferences/theme-name";

    public static void RegisterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(DarkModeCookiePath, (bool value, string? returnUrl, HttpContext ctx) =>
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
            };

            ctx.Response.Cookies.Append(DarkModeCookieName, value.ToString().ToLowerInvariant(), cookieOptions);

            var safeReturnUrl = string.IsNullOrWhiteSpace(returnUrl) || !Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)
                ? "/"
                : returnUrl;

            return Results.LocalRedirect(safeReturnUrl);
        })
        .AllowAnonymous();

        endpoints.MapGet(ThemeNameCookiePath, (string value, string? returnUrl, HttpContext ctx) =>
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
            };

            ctx.Response.Cookies.Append(ThemeNameCookieName, value.ToLowerInvariant(), cookieOptions);

            var safeReturnUrl = string.IsNullOrWhiteSpace(returnUrl) || !Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)
                ? "/"
                : returnUrl;

            return Results.LocalRedirect(safeReturnUrl);
        })
        .AllowAnonymous();
    }

    public static bool TryGetDarkModeFromCookie(this HttpContext? context, out bool isDarkMode)
    {
        isDarkMode = false;
        if (context == null) return false;

        if (context.Request.Cookies.TryGetValue(DarkModeCookieName, out var darkModeValue))
        {
            return bool.TryParse(darkModeValue, out isDarkMode);
        }

        return false;
    }

    public static bool TryGetThemeNameFromCookie(this HttpContext? context, out string? themeName)
    {
        themeName = null!;
        if (context == null) return false;

        return context.Request.Cookies.TryGetValue(ThemeNameCookieName, out themeName);
    }

    //public static MudBlazor.MudTheme GetTheme(string themeName)
    //{
    //    string key = GetThemeName(themeName);
    //    return PrimaryTheme.All[key];
    //}

    //public static string GetThemeName(string? themeName)
    //{
    //    if (themeName.IsEmpty()) return PrimaryTheme.DefaultThemeName;

    //    if (PrimaryTheme.AllNames.TryGetValue(themeName ?? string.Empty, out var name)) return name;

    //    return PrimaryTheme.DefaultThemeName;
    //}


    public static void UpdateDarkMode(this NavigationManager navManager, bool isDarkMode)
    {
        navManager.NotNull();

        var relativePath = navManager.ToBaseRelativePath(navManager.Uri);
        var returnUrl = relativePath.IsEmpty() ? "/" : $"/{relativePath}";
        var url = $"{DarkModeCookiePath}?value={isDarkMode.ToString().ToLowerInvariant()}&returnUrl={Uri.EscapeDataString(returnUrl)}";

        navManager.NavigateTo(url, forceLoad: true);
    }

    public static void UpdateThemeName(this NavigationManager navManager, string themeName)
    {
        navManager.NotNull();

        var relativePath = navManager.ToBaseRelativePath(navManager.Uri);
        var returnUrl = relativePath.IsEmpty() ? "/" : $"/{relativePath}";
        var url = $"{ThemeNameCookiePath}?value={themeName.ToLowerInvariant()}&returnUrl={Uri.EscapeDataString(returnUrl)}";

        navManager.NavigateTo(url, forceLoad: true);
    }
}
