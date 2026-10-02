using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Razor.Tools;


public static class NavTool
{
    public static string? GetReturnUrlFromUri(this NavigationManager navigationManager)
    {
        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);

        return query.TryGetValue("returnUrl", out var returnUrl) && returnUrl.ToString().IsNotEmpty()
            ? returnUrl.ToString()
            : null;
    }

    public static string AppendReturn(this NavigationManager navigationManager, string path)
    {
        navigationManager.NotNull();
        path.NotEmpty();

        var returnUrl = navigationManager.Uri.NotEmpty();

        var normalizedPath = path.TrimStart('/').TrimEnd('/');
        normalizedPath = $"{normalizedPath}?returnUrl={Uri.EscapeDataString(returnUrl)}";

        return normalizedPath;
    }

    public static string ReplaceUrl(this NavigationManager navigationManager, string path)
    {
        navigationManager.NotNull();
        path.NotEmpty();

        var normalizedPath = path.TrimStart('/').TrimEnd('/');

        var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);

        string? newUrl = (query.TryGetValue("returnUrl", out var returnUrl) && returnUrl.ToString() is not null) switch
        {
            true => $"{normalizedPath}?returnUrl={Uri.EscapeDataString(returnUrl.ToString())}",
            false => normalizedPath,
        };

        return newUrl;
    }

    public static bool IsUrlActive(this NavigationManager nav, string href, NavLinkMatch Match)
    {
        var current = new Uri(nav.Uri).AbsolutePath.TrimEnd('/');
        var target = nav.ToAbsoluteUri(href).AbsolutePath.TrimEnd('/');

        if (current.Length == 0) current = "/";
        if (target.Length == 0) target = "/";

        var isActive = Match == NavLinkMatch.All
            ? string.Equals(current, target, StringComparison.OrdinalIgnoreCase)
            : string.Equals(current, target, StringComparison.OrdinalIgnoreCase)
                || current.StartsWith(target + "/", StringComparison.OrdinalIgnoreCase);

        return isActive;
    }
}
