using Microsoft.AspNetCore.Components.Authorization;
using Toolbox.Security;
using Toolbox.Tools;

namespace Toolbox.Razor.Application;

public class AuthenticationAccess : IAuthAccess
{
    private readonly AuthenticationStateProvider _authProvider;

    public AuthenticationAccess(AuthenticationStateProvider authProvider) => _authProvider = authProvider.NotNull();

    public async Task<string> GetDisplayName()
    {
        var authState = await _authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        bool isAuthenticated = user.Identity?.IsAuthenticated == true;
        if (!isAuthenticated) return "Not authorized";

        var authUser = user.GetAuthUser();
        return authUser?.GetDisplayName() ?? "Unknown user";
    }

    public async Task<string> GetUserName()
    {
        var authState = await _authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        bool isAuthenticated = user.Identity?.IsAuthenticated == true;
        if (!isAuthenticated) return "Not authorized";

        var authUser = user.GetAuthUser();
        return authUser?.DisplayName ?? "Unknown user";
    }

    public async Task<string?> GetEmail()
    {
        var authState = await _authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        bool isAuthenticated = user.Identity?.IsAuthenticated == true;
        if (!isAuthenticated) return "Not authorized";

        var authUser = user.GetAuthUser();
        return authUser?.Email;
    }
}
